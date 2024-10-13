using System.Collections.Generic;

namespace ESMetadata.Models
{
    public class LinkOption
    {
        public string Type { get; set; }
        public string OriginalPath { get; set; }
        public List<string> AlternatePaths { get; set; }
        public bool IsOriginalSelected { get; set; } = true;

        public LinkOption( string type, string originalPath, List<string> alternatePaths)
        {
            Type = type;
            OriginalPath = originalPath;
            AlternatePaths = alternatePaths;
            IsOriginalSelected = OriginalPath != null;

        }
    }
}