using System.Collections.Generic;
using System.Linq;
using ESMetadata.Models.Gamelist;
using ESMetadata.Extensions;
using Playnite.SDK.Plugins;

namespace ESMetadata.Models.ESGame
{

    public enum LinkField
    {
        None,
        VideoTrailer,
        Logo,
        Bezel,
        Fanart,
        Boxback,
        Manual,
        Map,
        Magazine,
    };

    public class ESGameField
    {
        public MetadataField Field;
        public GamelistField Source;
        public LinkField LinkName;
        public string Value;

        public ESGameField(MetadataField field, GamelistField source, LinkField linkName = default, string value=default )
        {
            Field = field;
            Source = source;
            LinkName = linkName;
            Value = value;
        }
    }

    public class ESGameFields : List<ESGameField>
    {
        public ESGameFields() { }
        public ESGameFields(List<ESGameField> from) =>  AddRange(from);

        public void Add( MetadataField field, GamelistField source, LinkField linkName = LinkField.None)
        {
            Add( new ESGameField( field, source, linkName ) );
        }
        public bool AddMissing( ESGameField game)
        {
            if (!this.Any(
                f => f.Field == game.Field
                            && f.LinkName == game.LinkName
                            && f.Value.Equal(game.Value )))
            {
                Add(game);
                return true;
            }
            return false;
        }
    }
}