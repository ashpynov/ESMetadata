using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
using System.Globalization;
using ESMetadata.Models;
using ESMetadata.Settings;
using ESMetadata.ViewModels;
using ESMetadata.Models.ESGame;
using ESMetadata.Extensions;

namespace ESMetadata
{
    public class ESMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions Options;
        private readonly IPlayniteAPI PlayniteApi;
        private readonly ESMetadataSettings Settings;
        private readonly ESMetadataLibrary esLibrary;

        public override List<MetadataField> AvailableFields
        {
            get => esLibrary.Game.GetAvailableFields();
        }

        public ESMetadataProvider(MetadataRequestOptions options, ESMetadata plugin)
        {
            Options = options;
            PlayniteApi = ESMetadata.PlayniteApi;
            Settings = plugin.GetSettings();
            esLibrary = new ESMetadataLibrary(plugin, options.GameData, options.IsBackgroundDownload);
        }

        private IEnumerable<MetadataProperty> ToMetadataProperty(string property, char spliter = default)
        {
            return !property.IsNullOrEmpty()
                ? property.Split(spliter)
                          .Where(s => s.Trim().Length > 0 )
                          .Select(s => new MetadataNameProperty(s.Trim()))
                          .ToList()
                : default;
        }

        private int? ToMetadataScore(string rating)
        {
            return !rating.IsNullOrEmpty() && float.TryParse(rating, NumberStyles.Float, CultureInfo.InvariantCulture, out float score)
            ? (int)(score * 100) : default;
        }

        MetadataFile ScaledImage(string path, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        {
            if (maxWidth == int.MaxValue && maxHeight == int.MaxValue)
            {
                return new MetadataFile(path);
            }

            using (Image originalImage = Image.FromFile(path))
            {
                int goalWidth = Math.Min(originalImage.Width, maxWidth);
                int goalHeight = Math.Min(originalImage.Height, maxHeight);

                double scaleX = (double)goalWidth / originalImage.Width;
                double scaleY = (double)goalHeight / originalImage.Height;
                double scale = Math.Min(1.0, Math.Min(scaleX, scaleY));

                if (scale == 1.0)
                    return new MetadataFile(path);

                using (Bitmap newImage = new Bitmap(originalImage, (int)(scale * originalImage.Width), (int)(scale * originalImage.Height)))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        newImage.Save(memoryStream, ImageFormat.Png);
                        return new MetadataFile(Path.GetFileName(path), memoryStream.ToArray());
                    }
                }
            }
        }

        private MetadataFile ToMetadataFile(string path, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        {
            return !path.IsNullOrEmpty() && File.Exists(path) ? ScaledImage(path, maxWidth, maxHeight) : default;
        }

        private string ChooseImageFile(List<string> paths, string caption = null, int maxWidth = 240, int maxHeight = 180, string original = null)
        {
            if (paths.IsNullOrSingle() || (Options.IsBackgroundDownload && Settings.SelectAutomaticly))
                return paths.FirstOrDefault();

            List<string> distinct = DistinctResources(paths, original);

            if (distinct.IsNullOrSingle())
                return distinct.FirstOrDefault();

            double itemWidth = 240;
            double itemHeight = 180;

            if (maxWidth != int.MaxValue && maxHeight != int.MaxValue)
            {
                double zoom = Math.Min(itemWidth / maxWidth, itemHeight / maxHeight);
                itemWidth = maxWidth * zoom;
                itemHeight = maxHeight * zoom;

            }
            string result = PlayniteApi.Dialogs.ChooseImageFile(
                distinct.Select(p => new ImageFileOption(p)).ToList(),
                caption, itemWidth, itemHeight)?.Path;

            return !original.IsNullOrEmpty() && result.Equal(original) ? null : result;
        }

        private string ChooseVideoFile(List<string> paths, string caption = null, string original = null)
        {
            if (paths.IsNullOrSingle() || (Options.IsBackgroundDownload && Settings.SelectAutomaticly))
                return paths.FirstOrDefault();

            List<string> distinct = DistinctResources(paths, original);

            if (distinct.IsNullOrSingle())
                return distinct.FirstOrDefault();

            string result = VideoSelectionViewModel.ChooseVideoFile(
                distinct.Select(p => new VideoFileOption(p)).ToList(),
                caption, 320, 180)?.Path;

            return !original.IsNullOrEmpty() && result.Equal(original) ? null : result;

        }

        private MetadataFile ToMetadataFile(string name, List<string> paths, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        => ToMetadataFile(
                ChooseImageFile(
                    paths,
                    ResourceProvider.GetString($"LOC_ESMETADATA_Select{name}"),
                    maxWidth, maxHeight),
                maxWidth, maxHeight
        );

        public override string GetName(GetMetadataFieldArgs args)
        => esLibrary.Game.GetField(MetadataField.Name);

        public override string GetDescription(GetMetadataFieldArgs args)
        => esLibrary.Game.GetField(MetadataField.Description);

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.CoverImage.ToString(),
            esLibrary.Game.GetMultiField(MetadataField.CoverImage),
            Settings.GetCoverImageMaxSize().Width, Settings.GetCoverImageMaxSize().Height);

        public override MetadataFile GetIcon(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.Icon.ToString(),
            esLibrary.Game.GetMultiField(MetadataField.Icon),
            Settings.GetIconMaxSize().Width, Settings.GetIconMaxSize().Height);

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.BackgroundImage.ToString(),
            esLibrary.Game.GetMultiField(MetadataField.BackgroundImage),
            Settings.GetBackgroundImageMaxSize().Width, Settings.GetBackgroundImageMaxSize().Height);

        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Genres), '/');

        public override IEnumerable<MetadataProperty> GetRegions(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Region), '/');

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            string value = esLibrary.Game.GetField(MetadataField.ReleaseDate);
            if (!value.IsNullOrEmpty()
                && DateTime.TryParseExact(value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
            )
            {
                return new ReleaseDate(date);
            }
            return default;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esLibrary.Game.GetField(MetadataField.CriticScore));

        public override int? GetCommunityScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esLibrary.Game.GetField(MetadataField.CommunityScore));

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Developers), '/');

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.Game.GetField(MetadataField.Publishers), '/');

        static private string TempLinkName(string name) => $"[ESMS {name}]";
        static private string TempLinkName(LinkField field) => TempLinkName(field.ToString());


        static private string TranslateName(string name) => ResourceProvider.GetString($"LOC_ESMETADATA_{name}");
        static private string TranslateName(LinkField field) => TranslateName(field.ToString());

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {

            IEnumerable<LinkField> types = Settings.ProcessLinkFields;

            IEnumerable<string> removeLinks = Enum.GetNames(typeof(LinkField)).Select(n => TempLinkName(n));

            List<Link> links= Options.GameData
                ?.Links
                ?.Where(l => !removeLinks.Contains(l.Name))
                ?.ToList()
                ?? new List<Link>();

            if (!Settings.CopyExtraMetadata)
            {
                return links.AllOrDefault();
            }

            foreach (LinkField type in Settings.ImportAsLinkFields )
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
                        var destPath = getMediaFilePath(Options.GameData, newLinks.First().Url, type.ToString());
                        if (Options.GameData.Links.Any(l=>l.Name.Equal(TranslateName(type)) && l.Url.Equal(destPath)))
                        {
                            newLinks.First().Url = destPath;
                        }
                    }

                    links.RemoveAll(l => l.Name.Equal(TranslateName(type)));
                    links.AddMissing(newLinks);
                };
            }

            foreach (LinkField type in Settings.CopyExtraMetadataFields )
            {
                List<string> files = esLibrary.Game
                                .GetMultiField(MetadataField.Links, type);

                string original = getMediaFilePath(Options.GameData, files.FirstOrDefault(), type.ToString());
                if (!Settings.Overwrite && Options.IsBackgroundDownload && File.Exists(original))
                {
                    continue;
                }

                if(!files.IsNullOrEmpty())
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
                if( !manual.IsNullOrEmpty())
                {
                    Options.GameData.Manual = manual;
                }
            }
            return links.AllOrDefault();
        }

        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        {
            if (
                Settings.ImportFavorite
             && esLibrary.Game.GetField(MetadataField.Tags).Equal("true")
             && !Options.GameData.Favorite)
             {
                Options.GameData.Favorite = true;
             }

            return default;
        }

        static private void CopyExtraMetadata(Game game, string path, string type)
        {

            if (path.IsNullOrEmpty() || !File.Exists(path))
            {
                return;
            }

            string destPath = getMediaFilePath(game, path, type);

            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            File.Copy(path, destPath, true);
        }


        public override void Dispose()
        {
        }

        static private string getMediaFilePath(Game game, string path, string type)
        {
            if (path.IsNullOrEmpty())
            {
                return getDefaultMediaFilePath(game, type.ToEnum<LinkField>());
            }
            return Path.Combine(getGameExtraMetadataPath(game), type + Path.GetExtension(path));
        }

        static private string getGameExtraMetadataPath(Game game)
        {
            return Path.Combine(ESMetadata.PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "Games", game.Id.ToString());
        }

        static private string getDefaultMediaFilePath(Game game, LinkField type)
        {
            string path =  Path.Combine(getGameExtraMetadataPath(game), type.ToString());
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
                        && !newLink.Equal(getMediaFilePath(update.NewData, newLink, linkType.ToString()))
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
                                link.Url = getMediaFilePath(update.NewData, link.Url, linkType.ToString());
                            }
                        }
                    }
                    if ( linkType == LinkField.Manual
                        && ESMetadata.Settings.ImportManual
                        && !ESMetadata.Settings.KeepOriginalLinkPaths
                        && !newLink.IsNullOrEmpty()
                        && newLink.Equal(update.NewData.Manual))
                    {
                        update.NewData.Manual = getMediaFilePath(update.NewData, newLink, linkType.ToString());
                    }

                }
                update.NewData.Links = newLinks
                    .Where(l => !Enum.GetNames(typeof(LinkField))
                        .Select(t => TempLinkName(t))
                        .Contains(l.Name)).ToObservable();


            }
        }

        static public List<string> DistinctResources(List<string> paths, string original = null)
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
}