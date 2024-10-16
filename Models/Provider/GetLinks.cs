using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ESMetadata.Models.ESGame;
using ESMetadata.Extensions;


namespace ESMetadata.Models.Provider
{
    public partial class ESMetadataProvider : OnDemandMetadataProvider
    {
        private IEnumerable<Link> GetLinks()
        {
            IEnumerable<LinkField> types = Settings.ProcessLinkFields;

            IEnumerable<string> removeLinks = Enum.GetNames(typeof(LinkField)).Select(n => TempLinkName(n));

            List<Link> links = Options.GameData
                ?.Links
                ?.Where(l => !removeLinks.Contains(l.Name))
                ?.ToList()
                ?? new List<Link>();

            if (!Settings.CopyExtraMetadata)
            {
                return links.AllOrDefault();
            }

            foreach (LinkField type in Settings.ImportAsLinkFields)
            {
                List<Link> newLinks = esLibrary.Game
                                .GetMultiField(MetadataField.Links, type)
                                ?.Select(p => new Link(TranslateName(type), p))
                                ?.ToList();

                if (!newLinks.IsNullOrEmpty())
                {
                    if (Settings.CopyExtraMetadataFields.Contains(type)
                         && !Settings.KeepOriginalLinkPaths
                         && newLinks.Count() == 1)
                    {
                        var destPath = GetMediaFilePath(Options.GameData, newLinks.First().Url, type.ToString());
                        if (Options.GameData.Links.Any(l => l.Name.Equal(TranslateName(type)) && l.Url.Equal(destPath)))
                        {
                            newLinks.First().Url = destPath;
                        }
                    }

                    links.RemoveAll(l => l.Name.Equal(TranslateName(type)));
                    links.AddMissing(newLinks);
                };
            }

            foreach (LinkField type in Settings.CopyExtraMetadataFields)
            {
                List<string> files = esLibrary.Game
                                .GetMultiField(MetadataField.Links, type);

                string original = GetMediaFilePath(Options.GameData, files.FirstOrDefault(), type.ToString());
                if (!Settings.Overwrite && Options.IsBackgroundDownload && File.Exists(original))
                {
                    continue;
                }

                if (!files.IsNullOrEmpty())
                {
                    string selected = null;
                    switch (type)
                    {
                        case LinkField.VideoTrailer:
                            selected = ChooseVideoFile(files, ResourceProvider.GetString($"LOC_ESMETADATA_Select{type}"), original: original);
                            break;
                        case LinkField.Bezel:
                        case LinkField.Fanart:
                        case LinkField.Logo:
                        case LinkField.Boxback:
                            selected = ChooseImageFile(files, ResourceProvider.GetString($"LOC_ESMETADATA_Select{type}"), original: original);
                            break;
                        default:
                            links.AddMissing(files.Select(p => new Link(TempLinkName(type), p)));
                            break;
                    }
                    if (!selected.IsNullOrEmpty() && !selected.Equal(original))
                    {
                        links.AddMissing(files.Select(p => new Link(TempLinkName(type), selected)));
                    }
                }
            }

            if (Settings.ImportManual)
            {
                var manual = esLibrary.Game.GetField(MetadataField.Links, LinkField.Manual);
                if (!manual.IsNullOrEmpty())
                {
                    Options.GameData.Manual = manual;
                }
            }
            return links.AllOrDefault();
        }
    }
}