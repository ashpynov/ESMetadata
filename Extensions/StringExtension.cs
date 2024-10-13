using System;

namespace ESMetadata.Extensions
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool Equal(this string source, string second)
        {
            return 0 == string.Compare(source ?? "", second ?? "", StringComparison.OrdinalIgnoreCase);
        }
        public static TEnum ToEnum<TEnum>(this string source, TEnum fallback = default) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(source, out TEnum o))
            {
                return o;
            }
            return default;
        }
    }
};
