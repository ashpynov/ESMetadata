using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ESMetadata.Settings;

namespace ESMetadata
{
    public class ESMetadata : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private ESMetadataSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("71f952a8-4763-41e3-9934-7fe02a1e33d4");

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.Name,             // mame
            MetadataField.Description,      // desc
            MetadataField.CoverImage,       // image,thumbnail
            MetadataField.Icon,             // image,thumbnail
            MetadataField.BackgroundImage,  // image
            MetadataField.Genres,           // genre
            MetadataField.Region,           // region
            MetadataField.ReleaseDate,      // releasedate
            MetadataField.CriticScore,      // rating
            MetadataField.CommunityScore,   // rating
            MetadataField.Developers,       // developer
            MetadataField.Publishers,       // publisher
            MetadataField.Links,            // video, marquee
            MetadataField.Tags,             // favorites
        };

        public override string Name => "EmulationStation";

        public ESMetadata(IPlayniteAPI api) : base(api)
        {
            Settings = new ESMetadataSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new ESMetadataProvider(options, this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ESMetadataSettingsView();
        }

        public ESMetadataSettings GetSettings()
        {
            return Settings.Settings;
        }
    }
}