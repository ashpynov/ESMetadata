using ESMetadata.Extensions;
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


namespace ESMetadata.Settings
{

    public partial class ESMetadataSettings : ObservableObject
    {
        private ImageSourceField iconSource;
        public ImageSourceField IconSource
        {
            get => iconSource;
            set => SetValue(ref iconSource, value);
        }

        private ImageSourceField coverImageSource;
        public ImageSourceField CoverImageSource
        {
            get => coverImageSource;
            set => SetValue(ref coverImageSource , value);
        }

        private ImageSourceField backgroundImageSource;
        public ImageSourceField BackgroundImageSource
        {
            get => backgroundImageSource;
            set => SetValue(ref backgroundImageSource, value);
        }

        [DontSerialize]
        public bool CopyExtraMetadata
        {
            get => CopyExtraMetadataTriple != false;
            set => CopyExtraMetadataTriple = value;
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

        public class LinkFieldOptions : ObservableObject
        {
            public string Name { get; set; }

            [DontSerialize]
            public string TranslateName { get => ResourceProvider.GetString($"LOC_ESMETADATA_Download{Name}"); }

            private bool isChecked = true;
            public bool IsChecked
            {
                get => isChecked;
                set => SetValue(ref isChecked, value);
            }
        }

        private ObservableCollection<LinkFieldOptions> extraMetadataOptions;
        public ObservableCollection<LinkFieldOptions> ExtraMetadataOptions
        {
            get => extraMetadataOptions ?? (ExtraMetadataOptions = null);
            set
            {
                extraMetadataOptions = Enum.GetValues(typeof(LinkField))
                    .Cast<LinkField>()
                    .Where(n => n != LinkField.None)
                    .Select(linkField
                        => new LinkFieldOptions()
                        {
                            Name = linkField.ToString(),
                            IsChecked =
                                value?.FirstOrDefault(o => o.Name == linkField.ToString())?.IsChecked ?? true,
                        })
                    .ToObservable();
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public ObservableCollection<LinkField> CopyExtraMetadataFields
        {
            get => ExtraMetadataOptions
                .Where(o => o.IsChecked)
                .Select(o => o.Name.ToEnum<LinkField>())
                .ToObservable();
        }

        [DontSerialize]
        public RelayCommand CheckBoxChanged
        {
            get => new RelayCommand(
            () => {
                OnPropertyChanged(nameof(CopyExtraMetadataTriple));
                OnPropertyChanged(nameof(CopyExtraMetadata));
            });
        }

        [DontSerialize]
        public bool? CopyExtraMetadataTriple
        {
            get
            {
                if (ExtraMetadataOptions?.All(o => o.IsChecked)?? true)
                {
                    return true;
                }
                else if (!ExtraMetadataOptions.Any(o => o.IsChecked))
                {
                    return false;
                }
                return null;
            }
            set
            {
                foreach( LinkFieldOptions o in ExtraMetadataOptions)
                {
                    o.IsChecked = value == true;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(CopyExtraMetadata));
            }
        }
        private Size Unlimited = new Size(int.MaxValue, int.MaxValue);

        public Size GetIconMaxSize() => DownscaleImage ? new Size(IconMaxWidth, IconMaxHeight) : Unlimited;
        public Size GetCoverImageMaxSize() => DownscaleImage ? new Size(CoverImageMaxWidth, CoverImageMaxHeight) : Unlimited;
        public Size GetBackgroundImageMaxSize() => DownscaleImage ? new Size(BackgroundImageMaxWidth, BackgroundImageMaxHeight) : Unlimited;

        public bool BestMatchWithDesc { get; set; } = true;
        public bool IgnoreArticles { get; set; } = true;

        public bool NonStrictMediaSuggest { get; set; } = false;

        public void SetupSourceFields()
        {
            IconSource = ImageSourceField.SetupField(IconSource, MetadataField.Icon);
            CoverImageSource = ImageSourceField.SetupField(CoverImageSource, MetadataField.CoverImage);
            BackgroundImageSource = ImageSourceField.SetupField(BackgroundImageSource, MetadataField.BackgroundImage);
        }

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
            Settings.SetupSourceFields();
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