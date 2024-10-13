using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Playnite.SDK.Plugins;
using ESMetadata.Models.Gamelist;
using ESMetadata.Settings;
using ESMetadata.Extensions;

namespace ESMetadata.Models.ESGame
{
    public partial class ESGame
    {
        private readonly ESGameFields actualFieldMap;
        private readonly ESGameFields data = new ESGameFields();

        public ESGameFields Data { get => data;  }

        private readonly ESMetadataSettings Settings;

        public ESGame(ESMetadataSettings settings, GamelistGame game = default)
        {
            Settings = settings;

            List<MetadataField> customizableOrders = ImagesField;

            actualFieldMap = new ESGameFields(FieldMap
                .Where(f => f.Field != MetadataField.Tags || Settings.ImportFavorite)
                .ToList()
            );

            foreach( MetadataField f in customizableOrders)
            {
                AddCustomOrders(f);
            };

            data = new ESGameFields();
            if (game != default)
            {
                AddGameInfo(game);
            }
        }

        private void AddCustomOrders(MetadataField field)
        {
            PropertyInfo p = typeof(ESMetadataSettings).GetProperty($"{field}Source");
            if (p is null) return;

            actualFieldMap.RemoveAll(f => f.Field == field);

            ImageSourceField fieldSettings = p.GetValue(Settings) as ImageSourceField;
            actualFieldMap.AddRange(fieldSettings.Sources
                .Where(s => s.Enabled)
                .Select(s => FieldMap
                    .Where(f => f.Field == field && f.Source == s.Field)
                    .FirstOrDefault()
                )
            );
        }

        public void AddGameInfo(GamelistGame game, bool ImagesOnly = false)
        {
            if (game is null)
            {
                return;
            }

            foreach (ESGameField f in actualFieldMap.Where(f=> !ImagesOnly || ImagesSources.Contains(f.Source)))
            {
                if (f.Source == GamelistField.Name && game.Desc.IsNullOrEmpty() && Settings.BestMatchWithDesc)
                {
                    continue;
                }

                PropertyInfo prop = typeof(GamelistGame).GetProperty(f.Source.ToString());
                if (prop is null)
                {
                    continue;
                }
                string value = prop.GetValue(game) as string;

                if (f.Source == GamelistField.Favourites && value.IsNullOrEmpty() && Settings.ImportFavorite)
                    value = "False";

                if (value.IsNullOrEmpty())
                {
                    continue;
                }
                bool isImage = ImagesSources.Contains(f.Source);
                data.AddMissing(new ESGameField(f.Field, f.Source, f.LinkName, prop.GetValue(game) as string));
            }
        }


        public List<MetadataField> GetAvailableFields()
        => data.Where(f => !f.Value.IsNullOrEmpty()).Select(f => f.Field).Distinct().ToList();

        public List<string> GetMultiField(MetadataField field, LinkField link = LinkField.None)
        => data.Where(f => f.Field == field && f.LinkName == link).Select(f => f.Value).ToList();

        public string GetField(MetadataField field, LinkField link = LinkField.None)
        => GetMultiField(field, link).FirstOrDefault();
    };
}