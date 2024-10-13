using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ESMetadata.Models
{
    struct MediaFileInfo : IEquatable<MediaFileInfo>
    {
        public string FilePath { get; }
        public long FileSize { get; }

        private byte[] _header;
        private ulong? _headerCRC64;
        private string _md5Hash;
        private ulong? _md5CRC64;

        public MediaFileInfo(string filePath)
        {
            FilePath = filePath;
            FileSize = File.Exists(filePath) ? new FileInfo(filePath).Length : 0;

            _md5Hash = null;
            _md5CRC64 = null;
            _headerCRC64 = null;
            _header = null;
        }

        public byte[] Header
        {
            get
            {
                if (_header == null )
                {
                    _header = GetHeader(FilePath);
                }
                return _header;
            }
        }

        public ulong HeaderCRC64
        {
            get
            {
                if (!_headerCRC64.HasValue)
                {
                    _headerCRC64 = ComputeCRC64(Header);
                }
                return _headerCRC64.Value;
            }
        }


        public string MD5Hash
        {
            get
            {
                if (_md5Hash == null)
                {
                    _md5Hash = ComputeMD5(FilePath);
                }
                return _md5Hash;
            }
        }

        public ulong CRC64MD5Hash
        {
            get
            {
                if (!_md5CRC64.HasValue)
                {
                    _md5CRC64 = ComputeCRC64(MD5Hash);
                }
                return _md5CRC64.Value;
            }
        }

        public bool Equals(MediaFileInfo other)
        {
            return FileSize == other.FileSize &&
                   FileSize != 0 &&
                   other.FileSize != 0 &&
                   HeaderCRC64 == other.HeaderCRC64 &&
                   Header.SequenceEqual(other.Header) &&
                   CRC64MD5Hash == other.CRC64MD5Hash &&
                   MD5Hash == other.MD5Hash;
        }

        public override bool Equals(object obj)
        {
            return obj is MediaFileInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (FileSize == 0)
            {
                return 0;
            }

            int hash = FileSize.GetHashCode();

            if (_md5CRC64.HasValue)
            {
                hash = (hash * 397) ^ _md5CRC64.Value.GetHashCode();
            }

            if (_header != null)
            {
                hash = (hash * 397) ^ _header.GetHashCode();
            }

            if (_md5Hash != null)
            {
                hash = (hash * 397) ^ _md5Hash.GetHashCode();
            }

            if (_md5CRC64.HasValue)
            {
                hash = (hash * 397) ^ _md5CRC64.Value.GetHashCode();
            }

            return hash;
        }


        static byte[] GetHeader(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[64];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        static string ComputeMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        static ulong ComputeCRC64(byte[] data)
        {
            const ulong polynomial = 0xC96C5795D7870F42;
            ulong crc = 0xFFFFFFFFFFFFFFFF;

            foreach (byte b in data)
            {
                crc ^= (ulong)b << 56;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000000000000000) != 0)
                    {
                        crc = (crc << 1) ^ polynomial;
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }

        static ulong ComputeCRC64(string data)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
            return ComputeCRC64(bytes);
        }
    }
}