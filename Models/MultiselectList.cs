using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ESMetadata.Models.Gamelist;
using ESMetadata.Models.ESGame;
using ESMetadata.Extensions;
using System.Runtime.CompilerServices;

namespace ESMetadata.Models
{
    public class MultiselectList : ObservableObject
    {
        public class MultiselectOption : ObservableObject
        {
            public MultiselectOption()
            { }

            public MultiselectOption(string name, bool enabled = true)
            {
                Name = name;
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

            private string name;

            public string Name
            {
                get => name;
                set => SetValue(ref name, value);
            }

            public string TranslatedName
            {
                get => ResourceProvider.GetString($"LOC_ESMETADATA_{name}");
            }

            private bool upIsEnabled = true;
            [DontSerialize]
            public bool UpIsEnabled { get => upIsEnabled; set => SetValue( ref upIsEnabled, value); }
            private bool downIsEnabled = true;
            [DontSerialize]
            public bool DownIsEnabled { get => downIsEnabled; set => SetValue( ref downIsEnabled, value); }
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point point);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DontSerialize]
        public RelayCommand<MultiselectOption> MoveSourceUpCommand
        {
            get => new RelayCommand<MultiselectOption>((a) =>
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
                updateButons();
            });
        }

        [DontSerialize]
        public RelayCommand<MultiselectOption> MoveSourceDownCommand
        {
            get => new RelayCommand<MultiselectOption>((a) =>
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
                updateButons();
            });
        }

        private void updateButons()
        {
            foreach(MultiselectOption a in Sources)
            {
                a.DownIsEnabled = a != Sources.LastOrDefault();
                a.UpIsEnabled = a != Sources.FirstOrDefault();
            }
        }
        public ObservableCollection<MultiselectOption> Sources
        {
            get; set;
        }

        [DontSerialize]
        public string SelectionText
        {
            get => string.Join(", ", Sources.Where(a => a.Enabled).Select(a => a.TranslatedName).ToArray());
        }

        [DontSerialize]
        public event EventHandler SettingsChanged;

        public MultiselectList(ObservableCollection<MultiselectOption> sources)
        {
            Sources = sources.GroupBy(s => s.Name).Select(g => g.First()).ToObservable();
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
            updateButons();
        }

        private void OnSettingsChanged()
        {
            OnPropertyChanged(nameof(SelectionText));
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }


        static public void SetValue(
            ref MultiselectList property,
            MultiselectList current,
            IEnumerable<string> options,
            [CallerMemberName] string propertyName = null,
            IEnumerable<string> defaultEnabled = null)
        {

            if (current?.Sources is null)
            {
                current = new MultiselectList(
                    options.Select(
                        s => new MultiselectOption(s, defaultEnabled?.Any(n => n.Equal(s)) ?? true)
                    ).ToObservable()
                );
            }
            else
            {
                List<MultiselectOption> missed = options
                    .Where(f => !current?.Sources?.Any(s => s.Name == f) ?? true)
                    .Select(s => new MultiselectOption(s, defaultEnabled is null || defaultEnabled.Any(n => n.Equal(s))))
                    .ToList();

                current.Sources.RemoveAll(x => !options.Any(f => f == x.Name));
                current.Sources.AddRange(missed);
            }
            property = current;
            property.OnPropertyChanged(propertyName);
        }

        // static public ImageSourceField SetupField(ImageSourceField source, List<string> options)
        // {
        //     //List<GamelistField> AvailableSources = ESGame.GetSourcesForField(field);

        //     if (source?.Sources is null)
        //     {
        //         source = new ImageSourceField(options.Select(s => new SourceField(s)).ToObservable());
        //         return source;
        //     }

        //     List<SourceField> missed = options
        //         .Where(f => !source?.Sources?.Any(s => s.Name == f) ?? true)
        //         .Select(s => new SourceField(s))
        //         .ToList();

        //     source.Sources.RemoveAll(x => !options.Any(f => f == x.Name));
        //     source.Sources.AddRange(missed);
        //     return source;
        // }
    }
}