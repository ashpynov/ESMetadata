using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ESMetadata.Models.ESGame;
using ESMetadata.Extensions;

namespace ESMetadata.Models.Provider
{
    public partial class ESMetadataProvider : OnDemandMetadataProvider
    {

        static private string TempLinkName(string name) => $"[ESMS {name}]";
        static private string TempLinkName(LinkField field) => TempLinkName(field.ToString());
        static private string TranslateName(string name) => ResourceProvider.GetString($"LOC_ESMETADATA_{name}");
        static private string TranslateName(LinkField field) => TranslateName(field.ToString());

        static private string GetMediaFilePath(Game game, string path, string type)
        => path.IsNullOrEmpty()
            ? GetDefaultMediaFilePath(game, type.ToEnum<LinkField>())
            : Path.Combine(GetGameExtraMetadataPath(game), type + Path.GetExtension(path));

        static private string GetGameExtraMetadataPath(Game game)
        => Path.Combine(ESMetadata.PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "Games", game.Id.ToString());

        static private string GetDefaultMediaFilePath(Game game, LinkField type)
        {
            string path =  Path.Combine(GetGameExtraMetadataPath(game), type.ToString());
            switch (type)
            {
                case LinkField.VideoTrailer:
                    return path + ".mp4";
                case LinkField.Bezel:
                case LinkField.Logo:
                    return path + ".png";
                case LinkField.Fanart:
                case LinkField.Boxback:
                    return path + ".png";
                case LinkField.Manual:
                    return path + ".pdf";
            }
            return path;
        }

        static private List<string> DistinctPaths(List<string> paths, string original = null)
        {
            List<string> workPaths =
                paths.Distinct()
                     .Where(p => File.Exists(p))
                     .ToList();

            if (!original.IsNullOrEmpty())
            {
                workPaths.Insert(0, original);
            }

            List<string> result = workPaths.Select(p => new MediaFileInfo(p))
                     .Distinct()
                     .Select(mi => mi.FilePath)
                     .ToList();

            return result;
        }

    }
};