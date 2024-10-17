using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using ESMetadata.Extensions;
using System.Security.Cryptography;

namespace ESMetadata.Models.Provider
{
    public partial class ESMetadataProvider : OnDemandMetadataProvider
    {
        static private IEnumerable<MetadataProperty> ToMetadataProperty(string property, char spliter = default)
        => !property.IsNullOrEmpty()
                ? property.Split(spliter)
                          .Where(s => s.Trim().Length > 0)
                          .Select(s => new MetadataNameProperty(s.Trim()))
                          .ToList()
                : default;

        static private int? ToMetadataScore(string rating)
        => !rating.IsNullOrEmpty() && float.TryParse(rating, NumberStyles.Float, CultureInfo.InvariantCulture, out float score)
            ? (int)(score * 100) : default;

        static private ReleaseDate? ToMetadataReleaseDate(string releaseDate)
        => !releaseDate.IsNullOrEmpty() && DateTime.TryParseExact(releaseDate, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
            ? new ReleaseDate(date) : default;

        private IEnumerable<MetadataProperty> ToMetadataRegions(string regions, char spliter = default)
        => ToMetadataProperty(regions, spliter)
            ?.Select(region => new MetadataNameProperty(
                PlayniteApi.Emulation.Regions
                    .FirstOrDefault(r => r.Codes.Any(c => c.Equal(region.ToString())))
                    ?.Name
                    ?? region.ToString()
            ))
            ?.ToList()
            ?? default;


        private MetadataFile ToMetadataFile(string path, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
         => !path.IsNullOrEmpty() && File.Exists(path) ? ScaledImage(path, maxWidth, maxHeight) : default;

        private MetadataFile ToMetadataFile(string name, List<string> paths, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        => ToMetadataFile(
                ChooseImageFile(
                    paths,
                    ResourceProvider.GetString($"LOC_ESMETADATA_Select{name}"),
                    maxWidth, maxHeight),
                maxWidth, maxHeight
        );

        static private MetadataFile ScaledImage(string path, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        {
            if (maxWidth == int.MaxValue && maxHeight == int.MaxValue)
            {
                return new MetadataFile(path);
            }

            using (Image originalImage = Image.FromFile(path))
            {
                int goalWidth = Math.Min(originalImage.Width, maxWidth);
                int goalHeight = Math.Min(originalImage.Height, maxHeight);

                double scaleX = (double)goalWidth / originalImage.Width;
                double scaleY = (double)goalHeight / originalImage.Height;
                double scale = Math.Min(1.0, Math.Min(scaleX, scaleY));

                if (scale == 1.0)
                    return new MetadataFile(path);

                using (Bitmap newImage = new Bitmap(originalImage, (int)(scale * originalImage.Width), (int)(scale * originalImage.Height)))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        newImage.Save(memoryStream, ImageFormat.Png);
                        return new MetadataFile(Path.GetFileName(path), memoryStream.ToArray());
                    }
                }
            }
        }
    }
}