using ESMetadata.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ESMetadata.Settings
{

    public static class ExtensionMethods
    {
        public static int RemoveAll<T>(
            this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            List<T> itemsToRemove = coll.Where(condition).ToList();

            foreach (T itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }
        public static int AddRange<T>(
            this ObservableCollection<T> coll, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                coll.Add(item);
            }

            return items.Count();
        }
    }

    public class ESMetadataSettings : ObservableObject
    {
        public class ESSourceField : ObservableObject
        {
            public ESSourceField()
            {}

            public ESSourceField(ESGameField field , bool enabled = true)
            {
                ESField = field;
                Enabled = enabled;
            }

            private bool enabled = true;
            public bool Enabled
            {
                get => enabled;
                set
                {
                    enabled = value;
                    OnPropertyChanged();
                }
            }

            private ESGameField esField;
            public ESGameField ESField
            {
                get => esField;
                set
                {
                    esField = value;
                    OnPropertyChanged();
                }
            }
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point point);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        public class SourceFieldSettings : ObservableObject
        {
            [DontSerialize]
            public RelayCommand<ESSourceField> MoveSourceUpCommand
            {
                get => new RelayCommand<ESSourceField>((a) =>
                {
                    var index = Sources.IndexOf(a);
                    if (Sources.Count > 1 && (index - 1) >= 0)
                    {
                        Sources.Remove(a);
                        Sources.Insert(index - 1, a);
                        OnPropertyChanged(nameof(Sources));
                        Point point = new Point();
                        GetCursorPos(ref point);
                        SetCursorPos(point.X, point.Y - 30);
                    }
                });
            }

            [DontSerialize]
            public RelayCommand<ESSourceField> MoveSourceDownCommand
            {
                get => new RelayCommand<ESSourceField>((a) =>
                {
                    var index = Sources.IndexOf(a);
                    if (Sources.Count > 1 && (index + 1) < Sources.Count)
                    {
                        Sources.Remove(a);
                        Sources.Insert(index + 1, a);
                        OnPropertyChanged(nameof(Sources));
                        Point point = new Point();
                        GetCursorPos(ref point);
                        SetCursorPos(point.X, point.Y + 30);
                    }
                });
            }

            public ObservableCollection<ESSourceField> Sources
            {
                get; set;
            }

            [DontSerialize]
            public string SelectionText
            {
                get => string.Join(", ", Sources.Where(a => a.Enabled).Select(a => a.ESField).ToArray());
            }

            [DontSerialize]
            public event EventHandler SettingsChanged;

            public SourceFieldSettings(ObservableCollection<ESSourceField> sources)
            {

                Sources = sources.GroupBy(s => s.ESField).Select(g => g.First()).ToObservable();
                Sources.CollectionChanged += (s, e) =>
                {
                    OnSettingsChanged();
                };

                foreach (var source in Sources)
                {
                    source.PropertyChanged += (s, e) =>
                    {
                        OnSettingsChanged();
                    };
                }
            }

            private void OnSettingsChanged()
            {
                OnPropertyChanged(nameof(SelectionText));
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        static private SourceFieldSettings SetupField(SourceFieldSettings source, MetadataField field)
        {
            var AvailableSources = ESGameOptions.GetSourcesForField(field);

            if (source?.Sources is null)
            {
                source = new SourceFieldSettings(AvailableSources.Select(s => new ESSourceField(s)).ToObservable());
                return source;
            }

            List<ESSourceField> missed = AvailableSources
                .Where(f => !source?.Sources?.Any(s => s.ESField == f) ?? true)
                .Select(s => new ESSourceField(s))
                .ToList();

            source.Sources.RemoveAll(x => !AvailableSources.Any(f => f == x.ESField));
            source.Sources.AddRange(missed);
            return source;

        }

        private SourceFieldSettings iconSourceField;

        public SourceFieldSettings IconSource
        {
            get => iconSourceField;
            set
            {
                iconSourceField = value;
                OnPropertyChanged();
            }
        }

        private SourceFieldSettings coverImageSource;

        public SourceFieldSettings CoverImageSource
        {
            get => coverImageSource;
            set
            {
                coverImageSource = value;
                OnPropertyChanged();
            }
        }

        private SourceFieldSettings backgroundImageSource;

        public SourceFieldSettings BackgroundImageSource
        {
            get => backgroundImageSource;
            set
            {
                backgroundImageSource = value;
                OnPropertyChanged();
            }
        }


        private bool copyExtraMetadataOnLinks = true;
        public bool CopyExtraMetadataOnLinks
        {
            get => copyExtraMetadataOnLinks;
            set
            {
                copyExtraMetadataOnLinks = value;
                OnPropertyChanged();
            }
        }
        public bool Overwrite { get; set; } = false;
        public bool ImportFavorite { get; set; } = true;

        private bool selectAutomaticly = true;
        public bool SelectAutomaticly
        {
            get => selectAutomaticly;
            set
            {
                selectAutomaticly = value;
                OnPropertyChanged();
            }
        }

        private bool downscaleImage = true;
        public bool DownscaleImage
        {
            get => downscaleImage;
            set
            {
                downscaleImage = value;
                OnPropertyChanged();
            }
        }

        public int IconMaxWidth { get; set; } = 200;
        public int IconMaxHeight { get; set; } = 200;

        public int CoverImageMaxWidth { get; set; } = 484;
        public int CoverImageMaxHeight { get; set; } = 680;
        public int BackgroundImageMaxWidth { get; set; } = 1920;
        public int BackgroundImageMaxHeight { get; set; } = 1080;

        private Size Unlimited = new Size(int.MaxValue, int.MaxValue);

        public Size GetIconMaxSize() => DownscaleImage ? new Size(IconMaxWidth, IconMaxHeight ) : Unlimited;
        public Size GetCoverImageMaxSize() => DownscaleImage ? new Size(CoverImageMaxWidth, CoverImageMaxHeight) : Unlimited;
        public Size GetBackgroundImageMaxSize() => DownscaleImage ? new Size(BackgroundImageMaxWidth, BackgroundImageMaxHeight) : Unlimited;

        public bool BestMatchWithDesc { get; set; } = true;
        public bool IgnoreArticles { get; set; } = true;

        public void SetupSourceFields()
        {
            IconSource = SetupField(IconSource, MetadataField.Icon);
            CoverImageSource = SetupField(CoverImageSource, MetadataField.CoverImage);
            BackgroundImageSource = SetupField(BackgroundImageSource, MetadataField.BackgroundImage);
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