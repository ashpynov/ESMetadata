using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Dynamic;
using System.Xml.Serialization;
using ESMetadata.Models;
using System.Xml;
using System.Globalization;
using System.Windows;
using ESMetadata.Settings;
using ESMetadata.Common;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Runtime.Remoting.Messaging;

namespace ESMetadata
{
    public class ESMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions Options;
        private readonly IPlayniteAPI PlayniteApi;
        private readonly ESMetadataSettings Settings;
        private ESMetadataLibrary esLibrary;

        private bool LinksRequested = false;
        private bool TagsRequested = false;

        private List<MetadataField> availableFields = default;
        public override List<MetadataField> AvailableFields
        {
             get
             {
                if (availableFields == default)
                {
                    availableFields = GetAvailableFields();
                }
                return availableFields;
            }
        }

        private List<MetadataField> GetAvailableFields()
        {
            LinksRequested = false;
            return esLibrary.GameData.GetAvailableFields();
        }

        public ESMetadataProvider(MetadataRequestOptions options, ESMetadata plugin)
        {
            Options = options;
            PlayniteApi = plugin.PlayniteApi;
            Settings = plugin.GetSettings();
            esLibrary = new ESMetadataLibrary(plugin, options.GameData);

            List<MetadataField> availableFields = esLibrary.GameData.GetAvailableFields();
            List<MetadataField> a = availableFields;
        }

        private IEnumerable<MetadataProperty> ToMetadataProperty(string property, char spliter = default)
        {
            return !string.IsNullOrEmpty(property)
                ? property.Split(spliter)
                          .Where(s => s.Trim().Length > 0 )
                          .Select(s => new MetadataNameProperty(s.Trim()))
                          .ToList()
                : default;
        }

        private int? ToMetadataScore(string rating)
        {
            return !string.IsNullOrEmpty(rating) && float.TryParse(rating, NumberStyles.Float, CultureInfo.InvariantCulture, out float score)
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
            return !string.IsNullOrEmpty(path) && File.Exists(path) ? ScaledImage(path, maxWidth, maxHeight) : default;
        }

        private string ChooseImageFile(List<string> paths, string caption = null, int maxWidth = 240, int maxHeight = 180)
        {
            double itemWidth = 240;
            double itemHeight = 180;

            if (maxWidth != int.MaxValue && maxHeight != int.MaxValue)
            {
                double zoom = Math.Min(itemWidth / maxWidth, itemHeight / maxHeight);
                itemWidth = maxWidth * zoom;
                itemHeight = maxHeight * zoom;

            }
            return PlayniteApi.Dialogs.ChooseImageFile(
                paths.Select(p => new ImageFileOption(p)).ToList(),
                caption, itemWidth, itemHeight)?.Path;

        }

        private MetadataFile ToMetadataFile(string name, List<string>paths, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        => ToMetadataFile(( paths?.Count > 1 && ( !Options.IsBackgroundDownload || !Settings.SelectAutomaticly ))
                ? ChooseImageFile( paths, ResourceProvider.GetString($"LOC_ESMETADATA_Select{name}"), maxWidth, maxHeight )
                : paths.FirstOrDefault(), maxWidth, maxHeight);

        private void AddLink(string name, string path, List<Link> links)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                links.Add(new Link(name, path));
        }

        public override string GetName(GetMetadataFieldArgs args)
        => esLibrary.GameData.GetField(MetadataField.Name);

        public override string GetDescription(GetMetadataFieldArgs args)
        => esLibrary.GameData.GetField(MetadataField.Description);

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.CoverImage.ToString(),
            esLibrary.GameData.GetMultiField(MetadataField.CoverImage),
            484, 680);

        public override MetadataFile GetIcon(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.Icon.ToString(),
            esLibrary.GameData.GetMultiField(MetadataField.Icon),
            200, 200);

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        => ToMetadataFile(
            MetadataField.BackgroundImage.ToString(),
            esLibrary.GameData.GetMultiField(MetadataField.BackgroundImage),
            1920, 1080);

        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.GameData.GetField(MetadataField.Genres), '/');

        public override IEnumerable<MetadataProperty> GetRegions(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.GameData.GetField(MetadataField.Region), '/');

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            string value = esLibrary.GameData.GetField(MetadataField.ReleaseDate);
            if (!string.IsNullOrEmpty(value)
                && DateTime.TryParseExact(value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
            )
            {
                return new ReleaseDate(date);
            }
            return default;
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esLibrary.GameData.GetField(MetadataField.CriticScore));

        public override int? GetCommunityScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esLibrary.GameData.GetField(MetadataField.CommunityScore));

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.GameData.GetField(MetadataField.Developers), '/');

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esLibrary.GameData.GetField(MetadataField.Publishers), '/');

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            List<Link> links = esLibrary.GameData.Data
                .Where(
                    f => f.Field == MetadataField.Links
                    && f.LinkName != GameLinkField.None
                    && !string.IsNullOrEmpty(f.Value)
                    && File.Exists(f.Value))
                .Select(f => new Link(f.LinkName.ToString(), f.Value))
                .ToList();

            LinksRequested = links.Count > 0;

            Options.GameData.Links.ForEach(l => {if (!links.Select(li => li.Name).Contains(l.Name)) links.Add(l); });

            return links.Count> 0 ? links : default;
        }

        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        {
            if ( Settings.ImportFavorite )
            {
                TagsRequested = true;
                return Options.GameData.Tags != null
                    ? Options.GameData.Tags.Select(t => new MetadataNameProperty(t.Name)).ToList() as IEnumerable<MetadataProperty>
                    : new List<MetadataProperty>();
            }
            return default;
        }

        private void CopyExtraMetadata(Game game, string path, string type, bool overwrite)
        {

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            string destPath = getPath(game, path, type);

            if (File.Exists(destPath) && !overwrite)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            File.Copy(path, destPath, overwrite);
        }

        private void CopyExtraMetadata(Game game, Link link, bool overwrite) => CopyExtraMetadata(game, link.Url, link.Name, overwrite);

        public override void Dispose()
        {
            Game game = PlayniteApi.Database.Games.FirstOrDefault(g => g.Id == Options.GameData.Id);

            if ( Options.IsBackgroundDownload
              && LinksRequested
              && Settings.CopyExtraMetadataOnLinks
            )
            {
                foreach (var linkField in esLibrary.GameData.Data.Where(
                    f => f.Field == MetadataField.Links && f.LinkName != GameLinkField.None && !string.IsNullOrEmpty(f.Value)))
                {
                    CopyExtraMetadata(Options.GameData, linkField.Value, linkField.LinkName.ToString(), Settings.Overwrite);
                }
            }

            if ( Options.IsBackgroundDownload
              && Settings.ImportFavorite
              && TagsRequested )
            {

                if (game != default
                    && Tools.Equal(esLibrary.GameData.GetField(MetadataField.Tags), "true")
                    && !game.Favorite)
                {
                    game.Favorite = true;
                    PlayniteApi.Database.Games.Update(game);
                }
            }
        }

        private string getPath(Game game, string path, string type)
        {
            string fileName = string.Join(".", new List<string>() { type, Path.GetExtension(path) });
            return Path.Combine(Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "Games", game.Id.ToString(), fileName));
        }

        private string getPath(Game game, Link link)
        {
            return getPath(game, link.Url, link.Name);
        }

        private bool IsChanged(Game game, Link link)
        {
            return Tools.FastFileCompare(link.Url, getPath(game, link));
        }
        public void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            foreach (var updatedGame in args.UpdatedItems)
            {
                List<string> linkTags = new List<string> { "videoTrailer", "Logo", "Bezel", "Fanart" };
                ObservableCollection<Link> newLinks = updatedGame.NewData.Links;
                ObservableCollection<Link> oldLinks = updatedGame.OldData.Links ?? new ObservableCollection<Link>();
                if (newLinks?.Count > 0)
                {
                    List<Link> updatedLinks = newLinks.Where(
                        link => !string.IsNullOrEmpty(link.Url)
                        && linkTags.Any(t => Tools.Equal(link.Name, t)
                        && IsChanged(updatedGame.NewData, link))).ToList();

                    if (updatedLinks.Count == 0) continue;

                    List<Link> linksToCopy = updatedLinks.Where(l => !File.Exists(getPath(updatedGame.NewData, l))).ToList();
                    List<Link> toConfirm = updatedLinks.Where(l => File.Exists(getPath(updatedGame.NewData, l))).ToList();
                    linksToCopy.AddRange(toConfirm);

                    foreach( Link link in linksToCopy)
                    {
                        CopyExtraMetadata(updatedGame.NewData, link, true);
                    }
                }
            }
        }
    }
}