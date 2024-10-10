using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using ESMetadata.Common;
using ESMetadata.Settings;
using Playnite.SDK.Plugins;

namespace ESMetadata.Models
{

    public enum ESGameField
    {
        Name,
        Desc,
        Image,
        Thumbnail,
        Marquee,
        Fanart,
        Genre,
        Region,
        ReleaseDate,
        Rating,
        Developer,
        Publisher,
        Video,
        Bezel,
        Favourites
    };

    public enum GameLinkField
    {
        None,
        VideoTrailer,
        Bezel,
        Fanart,
        Logo
    };

    public class GameField
    {
        public MetadataField Field;
        public ESGameField Source;
        public GameLinkField LinkName;
        public string Value;

        public GameField(MetadataField field, ESGameField source, GameLinkField linkName = default, string value=default )
        {
            Field = field;
            Source = source;
            LinkName = linkName;
            Value = value;
        }
    }

    public class GameFields : List<GameField>
    {
        public GameFields() { }
        public GameFields(List<GameField> from) =>  AddRange(from);

        public void Add( MetadataField field, ESGameField source, ESGameField source2 = default, ESGameField source3=default, ESGameField source4=default )
        {
            Add( new GameField( field, source ) );
            if ( source2 != default ) Add( new GameField( field, source2 ) );
            if ( source3 != default ) Add( new GameField( field, source3 ) );
            if ( source4 != default ) Add( new GameField( field, source4 ) );
        }
        public void Add( MetadataField field, ESGameField source, GameLinkField linkName)
        {
            Add( new GameField( field, source, linkName ) );
        }
        public bool AddMissing( GameField game)
        {
            if (!this.Any(
                f => f.Field == game.Field
                            && f.LinkName == game.LinkName
                            && Tools.Equals(f.Value, game.Value )))
            {
                Add(game);
                return true;
            }
            return false;
        }
    }


    public class ESGameOptions
    {
        static private GameFields FieldMap = new GameFields()
        {
            { MetadataField.Name,               ESGameField.Name },
            { MetadataField.Description,        ESGameField.Desc },
            { MetadataField.CoverImage,         ESGameField.Image, ESGameField.Thumbnail },
            { MetadataField.Icon,               ESGameField.Image, ESGameField.Thumbnail,  ESGameField.Marquee },
            { MetadataField.BackgroundImage,    ESGameField.Image, ESGameField.Fanart },
            { MetadataField.Genres,             ESGameField.Genre },
            { MetadataField.Region,             ESGameField.Region },
            { MetadataField.ReleaseDate,        ESGameField.ReleaseDate },
            { MetadataField.CriticScore,        ESGameField.Rating },
            { MetadataField.CommunityScore,     ESGameField.Rating },
            { MetadataField.Developers,         ESGameField.Developer },
            { MetadataField.Publishers,         ESGameField.Publisher },
            { MetadataField.Links,              ESGameField.Video,      GameLinkField.VideoTrailer },
            { MetadataField.Links,              ESGameField.Marquee,    GameLinkField.Logo },
            { MetadataField.Links,              ESGameField.Fanart,     GameLinkField.Fanart },
            { MetadataField.Links,              ESGameField.Fanart,     GameLinkField.Bezel },
            { MetadataField.Tags,               ESGameField.Favourites }
        };

        static public List<ESGameField> GetSourcesForField(MetadataField field) => FieldMap.Where(f => f.Field == field).Select(f => f.Source).ToList();
        private GameFields actualFieldMap;
        private GameFields data = new GameFields();

        public GameFields Data { get => data;  }

        ESMetadataSettings Settings;

        private void AddCustomOrders(MetadataField field)
        {
            PropertyInfo p = typeof(ESMetadataSettings).GetProperty($"{field}Source");
            if (p is null) return;

            actualFieldMap.RemoveAll(f => f.Field == field);

            ESMetadataSettings.SourceFieldSettings fieldSettings = p.GetValue(Settings) as ESMetadataSettings.SourceFieldSettings;
            actualFieldMap.AddRange(fieldSettings.Sources
                .Where(s => s.Enabled)
                .Select(s => FieldMap
                    .Where(f => f.Field == field && f.Source == s.ESField)
                    .FirstOrDefault()
                )
            );
        }
        public ESGameOptions(ESMetadataSettings settings, ESGame game = default)
        {
            Settings = settings;

            List<MetadataField> customizableOrders = new List<MetadataField>()
            {
                MetadataField.Icon,
                MetadataField.CoverImage,
                MetadataField.BackgroundImage
            };

            actualFieldMap = new GameFields(FieldMap
                .Where(f => f.Field != MetadataField.Tags || Settings.ImportFavorite)
                .ToList()
            );

            foreach( MetadataField f in customizableOrders)
            {
                AddCustomOrders(f);
            };

            data = new GameFields();
            if (game != default)
            {
                AddGame(game);
            }
        }

        public void AddGame(ESGame game)
        {
            if (game is null)
            {
                return;
            }

            foreach (GameField f in actualFieldMap)
            {
                if (f.Source == ESGameField.Name && string.IsNullOrEmpty(game.Desc) && Settings.BestMatchWithDesc)
                {
                    continue;
                }

                PropertyInfo prop = typeof(ESGame).GetProperty(f.Source.ToString());
                if (prop is null)
                {
                    continue;
                }
                string value = prop.GetValue(game) as string;

                if (f.Source == ESGameField.Favourites && string.IsNullOrEmpty(value) && Settings.ImportFavorite)
                    value = "False";

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                data.AddMissing(new GameField(f.Field, f.Source, f.LinkName, prop.GetValue(game) as string));
            }
        }

        static public List<MetadataField> GetSuportedFields()
        => FieldMap.Select(f => f.Field).Distinct().ToList();

        public List<MetadataField> GetAvailableFields()
        => data.Where(f => !string.IsNullOrEmpty(f.Value)).Select(f => f.Field).Distinct().ToList();

        public List<string> GetMultiField(MetadataField field, GameLinkField link = GameLinkField.None)
        => data.Where(f => f.Field == field && f.LinkName == link).Select(f => f.Value).ToList();

        public string GetField(MetadataField field, GameLinkField link = GameLinkField.None)
        => GetMultiField(field, link).FirstOrDefault();
    };
}