using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using ESMetadata.Models.ESGame;
using ESMetadata.Extensions;


namespace ESMetadata.Models.Provider
{
    public partial class ESMetadataProvider : OnDemandMetadataProvider
    {
        static public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            IEnumerable<LinkField> LinkTypes = Enum.GetValues(typeof(LinkField)).Cast<LinkField>();

            foreach (ItemUpdateEvent<Game> update in args.UpdatedItems)
            {
                ObservableCollection<Link> newLinks = update.NewData.Links;
                ObservableCollection<Link> oldLinks = update.OldData.Links ?? new ObservableCollection<Link>();
                if (newLinks.IsNullOrEmpty())
                {
                    continue;
                }

                foreach (LinkField linkType in LinkTypes)
                {
                    string oldLink = oldLinks.FirstOrDefault(l => l.Name.Equal(TempLinkName(linkType)))?.Url;
                    string newLink = newLinks.FirstOrDefault(l => l.Name.Equal(TempLinkName(linkType)))?.Url;

                    if (!newLink.IsNullOrEmpty()
                        && !newLink.Equal(oldLink)
                        && !newLink.Equal(GetMediaFilePath(update.NewData, newLink, linkType.ToString()))
                        && File.Exists(newLink)
                    )
                    {
                        CopyExtraMetadata(update.NewData, newLink, linkType.ToString());
                    }
                    if (!newLink.IsNullOrEmpty())
                    {
                        if (ESMetadata.Settings.ImportAsLinkFields.Contains(linkType)
                            && !ESMetadata.Settings.KeepOriginalLinkPaths)
                        {
                            var link = newLinks?.FirstOrDefault(l => l.Name.Equal(TranslateName(linkType)) && l.Url.Equal(newLink));
                            if (link != null)
                            {
                                link.Url = GetMediaFilePath(update.NewData, link.Url, linkType.ToString());
                            }
                        }
                    }
                    if (linkType == LinkField.Manual
                        && ESMetadata.Settings.ImportManual
                        && !ESMetadata.Settings.KeepOriginalLinkPaths
                        && !newLink.IsNullOrEmpty()
                        && newLink.Equal(update.NewData.Manual))
                    {
                        update.NewData.Manual = GetMediaFilePath(update.NewData, newLink, linkType.ToString());
                    }

                }
                update.NewData.Links = newLinks
                    .Where(l => !Enum.GetNames(typeof(LinkField))
                        .Select(t => TempLinkName(t))
                        .Contains(l.Name)).ToObservable();

            }
        }
        static private void CopyExtraMetadata(Game game, string path, string type)
        {

            if (path.IsNullOrEmpty() || !File.Exists(path))
            {
                return;
            }

            string destPath = GetMediaFilePath(game, path, type);

            Directory.CreateDirectory(Path.GetDirectoryName(destPath));

            // TODO stop currently played ExtraMetadata


            try
            {
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }
                File.Copy(path, destPath, true);
            }
            catch { }
        }
    }
}
