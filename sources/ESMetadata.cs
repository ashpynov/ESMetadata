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

        private ESMetadataSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("71f952a8-4763-41e3-9934-7fe02a1e33d4");

        public override List<MetadataField> SupportedFields { get; } = new List<MetadataField>
        {
            MetadataField.Description
            // Include addition fields if supported by the metadata source
        };

        // Change to something more appropriate
        public override string Name => "EmulationStation";

        public ESMetadata(IPlayniteAPI api) : base(api)
        {
            settings = new ESMetadataSettingsViewModel(this);
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
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ESMetadataSettingsView();
        }
    }
}