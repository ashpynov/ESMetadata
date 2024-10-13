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

namespace ESMetadata.Settings
{
    public class ImageSourceField : ObservableObject
    {
        public class SourceField : ObservableObject
        {
            public SourceField()
            { }

            public SourceField(GamelistField field, bool enabled = true)
            {
                Field = field;
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

            private GamelistField field;

            public string Name
            {
                get => field.ToString();
                set => value.ToEnum<GamelistField>();
            }

            [DontSerialize]
            public GamelistField Field
            {
                get => field;
                set
                {
                    field = value;
                    OnPropertyChanged();
                }
            }
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point point);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DontSerialize]
        public RelayCommand<SourceField> MoveSourceUpCommand
        {
            get => new RelayCommand<SourceField>((a) =>
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
        public RelayCommand<SourceField> MoveSourceDownCommand
        {
            get => new RelayCommand<SourceField>((a) =>
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

        public ObservableCollection<SourceField> Sources
        {
            get; set;
        }

        [DontSerialize]
        public string SelectionText
        {
            get => string.Join(", ", Sources.Where(a => a.Enabled).Select(a => a.Field).ToArray());
        }

        [DontSerialize]
        public event EventHandler SettingsChanged;

        public ImageSourceField(ObservableCollection<SourceField> sources)
        {

            Sources = sources.GroupBy(s => s.Field).Select(g => g.First()).ToObservable();
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

        static public ImageSourceField SetupField(ImageSourceField source, MetadataField field)
        {
            List<GamelistField> AvailableSources = ESGame.GetSourcesForField(field);

            if (source?.Sources is null)
            {
                source = new ImageSourceField(AvailableSources.Select(s => new SourceField(s)).ToObservable());
                return source;
            }

            List<SourceField> missed = AvailableSources
                .Where(f => !source?.Sources?.Any(s => s.Field == f) ?? true)
                .Select(s => new SourceField(s))
                .ToList();

            source.Sources.RemoveAll(x => !AvailableSources.Any(f => f == x.Field));
            source.Sources.AddRange(missed);
            return source;
        }
    }
}