using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;
using ESMetadata.Settings;
using ESMetadata.Models.ESGame;
using Playnite.SDK;
using Playnite.SDK.Plugins;


namespace ESMetadata
{
    public class ESMetadata : MetadataPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private static ESMetadataSettingsViewModel SettingsModel { get; set; }
        public static ESMetadataSettings Settings { get => SettingsModel.Settings; }

        public override Guid Id { get; } = Guid.Parse("71f952a8-4763-41e3-9934-7fe02a1e33d4");

        public override List<MetadataField> SupportedFields { get; } = ESGame.GetSuportedFields;

        public override string Name => "EmulationStation";

        private bool itemUpdatedRegistered = false;

        public static new IPlayniteAPI PlayniteApi;
        public ESMetadata(IPlayniteAPI api) : base(api)
        {
            PlayniteApi = api;
            SettingsModel = new ESMetadataSettingsViewModel(this);
            Properties = new MetadataPluginProperties
            {
                HasSettings = true
            };
            SettingsChangedEventHandler(null,null);

            SettingsModel.PropertyChanged += SettingsChangedEventHandler;

        }

        public void SettingsChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (itemUpdatedRegistered != SettingsModel.Settings.CopyExtraMetadata)
            {
                if (SettingsModel.Settings.CopyExtraMetadata)
                {
                    PlayniteApi.Database.Games.ItemUpdated += ESMetadataProvider.Games_ItemUpdated;
                }
                else
                {
                    PlayniteApi.Database.Games.ItemUpdated -= ESMetadataProvider.Games_ItemUpdated;
                }
                itemUpdatedRegistered = SettingsModel.Settings.CopyExtraMetadata;
            }
        }

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            return new ESMetadataProvider(options, this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return SettingsModel;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ESMetadataSettingsView();
        }

        public ESMetadataSettings GetSettings()
        {
            return SettingsModel.Settings;
        }
    }
}