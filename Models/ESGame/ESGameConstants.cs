using System.Collections.Generic;
using System.Linq;
using ESMetadata.Models.Gamelist;
using Playnite.SDK.Plugins;

namespace ESMetadata.Models.ESGame
{
    public partial class ESGame
    {
        private static readonly ESGameFields FieldMap = new ESGameFields()
        {
            { MetadataField.Name,               GamelistField.Name },
            { MetadataField.Description,        GamelistField.Desc },
            { MetadataField.CoverImage,         GamelistField.Thumbnail },
            { MetadataField.CoverImage,         GamelistField.Image },
            { MetadataField.Icon,               GamelistField.Thumbnail},
            { MetadataField.Icon,               GamelistField.Image},
            { MetadataField.Icon,               GamelistField.Marquee },
            { MetadataField.BackgroundImage,    GamelistField.Image},
            { MetadataField.BackgroundImage,    GamelistField.Fanart },
            { MetadataField.Genres,             GamelistField.Genre },
            { MetadataField.Region,             GamelistField.Region },
            { MetadataField.ReleaseDate,        GamelistField.ReleaseDate },
            { MetadataField.CriticScore,        GamelistField.Rating },
            { MetadataField.CommunityScore,     GamelistField.Rating },
            { MetadataField.Developers,         GamelistField.Developer },
            { MetadataField.Publishers,         GamelistField.Publisher },
            { MetadataField.Links,              GamelistField.Video,      LinkField.VideoTrailer },
            { MetadataField.Links,              GamelistField.Marquee,    LinkField.Logo },
            { MetadataField.Links,              GamelistField.Fanart,     LinkField.Fanart },
            { MetadataField.Links,              GamelistField.Bezel,      LinkField.Bezel },
            { MetadataField.Tags,               GamelistField.Favourites }
        };
        private static readonly List<MetadataField> ImagesField = new List<MetadataField>()
            {
                MetadataField.Icon,
                MetadataField.CoverImage,
                MetadataField.BackgroundImage
            };
        private static readonly List<GamelistField> ImagesSources
        = FieldMap
            .Where(f => ImagesField.Contains(f.Field) || f.Field == MetadataField.Links)
            .Select(f => f.Source)
            .Distinct()
            .ToList();


        static public readonly List<MetadataField> GetSuportedFields
        = FieldMap
            .Select(f => f.Field)
            .Distinct()
            .ToList();

        static public List<GamelistField> GetSourcesForField(MetadataField field)
        => FieldMap
            .Where(f => f.Field == field)
            .Select(f => f.Source)
            .ToList();


    }
}