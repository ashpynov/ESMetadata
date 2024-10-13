using Playnite.SDK;

namespace ESMetadata.Models
{
        /// <summary>
    /// Represents item for image selection dialog.
    /// </summary>
    public class VideoFileOption : GenericItemOption
    {
        /// <summary>
        /// Gets or sets image path or URL.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="VideoFileOption"/>.
        /// </summary>
        public VideoFileOption() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="VideoFileOption"/>.
        /// </summary>
        /// <param name="path"></param>
        public VideoFileOption(string path)
        {
            Path = path;
        }
    }
}
