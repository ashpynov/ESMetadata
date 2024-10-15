using ESMetadata.Extensions;
using ESMetadata.Models;
using ESMetadata.Models.ESGame;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Converters;


namespace ESMetadata.Settings
{

    public partial class ESMetadataSettings : ObservableObject
    {
        private MultiselectList iconSource;
        public MultiselectList IconSource
        {
            get => iconSource ?? (IconSource = null);
            set => MultiselectList.SetValue(
                ref iconSource,
                value,
                ESGame.GetSourceNamesForField(MetadataField.Icon)
            );
        }

        private MultiselectList coverImageSource;
        public MultiselectList CoverImageSource
        {
            get => coverImageSource ?? (CoverImageSource = null);
            set => MultiselectList.SetValue(
                ref coverImageSource,
                value,
                ESGame.GetSourceNamesForField(MetadataField.CoverImage)
            );
        }

        private MultiselectList backgroundImageSource;
        public MultiselectList BackgroundImageSource
        {
            get => backgroundImageSource ?? (BackgroundImageSource = null);
            set => MultiselectList.SetValue(
                ref backgroundImageSource,
                value,
                ESGame.GetSourceNamesForField(MetadataField.BackgroundImage)
            );
        }

        public bool Overwrite { get; set; } = false;
        public bool ImportFavorite { get; set; } = true;

        private bool selectAutomaticly = true;
        public bool SelectAutomaticly
        {
            get => selectAutomaticly;
            set => SetValue(ref selectAutomaticly, value);
        }

        private bool downscaleImage = true;
        public bool DownscaleImage
        {
            get => downscaleImage;
            set => SetValue(ref downscaleImage, value);
        }

        public int IconMaxWidth { get; set; } = 200;
        public int IconMaxHeight { get; set; } = 200;
        public int CoverImageMaxWidth { get; set; } = 484;
        public int CoverImageMaxHeight { get; set; } = 680;
        public int BackgroundImageMaxWidth { get; set; } = 1920;
        public int BackgroundImageMaxHeight { get; set; } = 1080;

        private MultiselectList importToExtraMetadata;
        public MultiselectList ImportToExtraMetadata
        {
            get => importToExtraMetadata ?? (ImportToExtraMetadata = null);
            set => MultiselectList.SetValue(
                ref importToExtraMetadata,
                value,
                ESGame.LinkFields.ToStrings(),
                defaultEnabled: ESGame.DefaultCopy.ToStrings()
            );
        }

        private bool copyExtraMetadata = true;
        public bool CopyExtraMetadata
        {
            get => copyExtraMetadata;
            set => SetValue(ref copyExtraMetadata, value);
        }

        private bool importAsLinks = true;
        public bool ImportAsLinks
        {
            get => importAsLinks;
            set => SetValue(ref importAsLinks, value);
        }

        private MultiselectList importAsLinksOptions;
        public MultiselectList ImportAsLinksOptions
        {
            get => importAsLinksOptions ?? (ImportAsLinksOptions = null);
            set => MultiselectList.SetValue(
                ref importAsLinksOptions,
                value,
                ESGame.LinkFields.ToStrings(),
                defaultEnabled: ESGame.DefaultAsLinks.ToStrings()
            );
        }

        public bool ImportManual { get; set; } = true;

        public bool KeepOriginalLinkPaths { get; set; } = true;

        [DontSerialize]
        public ObservableCollection<LinkField> CopyExtraMetadataFields
        {
            get => ImportToExtraMetadata.Sources
                .Where(o => CopyExtraMetadata && o.Enabled)
                .Select(o => o.Name.ToEnum<LinkField>())
                .ToObservable();
        }

        [DontSerialize]
        public ObservableCollection<LinkField> ImportAsLinkFields
        {
            get => ImportAsLinksOptions.Sources
                .Where(o => ImportAsLinks && o.Enabled)
                .Select(o => o.Name.ToEnum<LinkField>())
                .ToObservable();
        }

        [DontSerialize]
        public ObservableCollection<LinkField> ProcessLinkFields
        {
            get
            {
                ObservableCollection<LinkField> result = new ObservableCollection<LinkField>();
                result.AddMissing(CopyExtraMetadataFields);
                result.AddMissing(ImportAsLinkFields);
                if (ImportManual)
                {
                     result.AddMissing(LinkField.Manual);
                }
                return result;
            }
        }

        private Size Unlimited = new Size(int.MaxValue, int.MaxValue);

        public Size GetIconMaxSize() => DownscaleImage ? new Size(IconMaxWidth, IconMaxHeight) : Unlimited;
        public Size GetCoverImageMaxSize() => DownscaleImage ? new Size(CoverImageMaxWidth, CoverImageMaxHeight) : Unlimited;
        public Size GetBackgroundImageMaxSize() => DownscaleImage ? new Size(BackgroundImageMaxWidth, BackgroundImageMaxHeight) : Unlimited;

        public bool BestMatchWithDesc { get; set; } = true;
        public bool IgnoreArticles { get; set; } = true;
        public bool NonStrictMediaSuggest { get; set; } = false;

    }

    public class ESMetadataSettingsViewModel : ObservableObject, ISettings, INotifyPropertyChanged
    {
        private readonly ESMetadata plugin;
        private ESMetadataSettings editingClone { get; set; }

        private ESMetadataSettings settings;
        public ESMetadataSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ESMetadataSettingsViewModel(ESMetadata plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ESMetadataSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ESMetadataSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}