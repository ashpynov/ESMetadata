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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ESMetadata
{
    public class ESMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions Options;
        private readonly IPlayniteAPI PlayniteApi;
        private readonly ESMetadataSettings Settings;
        private readonly List<MetadataField> SupportedFields;

        private List<ESGame> ESGames = new List<ESGame>();

        private ESGame esGame;


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
            List<MetadataField> result = SupportedFields.Where(f =>
                default != typeof(ESMetadataProvider).GetMethod("Get" + f)?.Invoke(this, new object[] { new GetMetadataFieldArgs() })
            ).ToList();
            LinksRequested = false;

            return result;
        }

        public ESMetadataProvider(MetadataRequestOptions options, ESMetadata plugin)
        {
            Options = options;
            PlayniteApi = plugin.PlayniteApi;
            Settings = plugin.GetSettings();
            SupportedFields = plugin.SupportedFields;
            LoadGamelist(Options.GameData);
            esGame = FindGame(Options.GameData);
        }

        private void LoadGamelist(Game game)
        {
            // find gamelist.xml
            // take rom path and find closest
            // game not in list => search gamelists in "library"

            string romPath = GetRomPath(game);

            if (string.IsNullOrEmpty(romPath))
                return;

            string gamelistFile = Path.Combine(Path.GetDirectoryName(romPath),"gamelist.xml");

            while (!File.Exists(gamelistFile) && !string.IsNullOrEmpty(gamelistFile))
            {
                gamelistFile =
                    Path.GetDirectoryName(gamelistFile) != Path.GetPathRoot(gamelistFile)
                    ? Path.Combine(Directory.GetParent(Path.GetDirectoryName(gamelistFile)).FullName, Path.GetFileName(gamelistFile))
                    : default;
            }
            List<string> gameLists = new List<string>();
            if (string.IsNullOrEmpty(gamelistFile))
            {

            }
            else
            {
                gameLists.AddMissing(gamelistFile);
            }

            ESGames = new List<ESGame>();
            foreach(string file in gameLists)
            {
                XDocument doc = XDocument.Load(file);

                string root = Path.GetDirectoryName(file);

                foreach (var gameElement in doc.Descendants("game"))
                {
                    ESGames.Add(new ESGame(gameElement, root));
                }
            }
        }

        private string GetRomPath(Game game)
        {
            GameRom rom = game.Roms?.FirstOrDefault();
            if ((rom ?? default) == default)
                return default;

            Guid emulatorId = game.GameActions.FirstOrDefault(a => a.IsPlayAction && a.Type == GameActionType.Emulator)?.EmulatorId ?? default;
            string emulatorPath = PlayniteApi.Database.Emulators.Get(emulatorId)?.InstallDir ?? "";
            return PlayniteApi.ExpandGameVariables(game, rom.Path, emulatorPath).Replace("\\\\", "\\");
        }

        private string DeConventGameName(string name)
        {
            int len = name.IndexOfAny(new char[] { '(', '[' });
            return Regex.Replace((len > 0 ? name.Substring(0, len) : name).Trim(), @"[^A-Za-z0-9]+", "");
        }

        private int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }

        private double Similarity(string a, string b)
        {
            string first = DeConventGameName(a.ToLower());
            string second = DeConventGameName(b.ToLower());

            if ((first == null) || (first == null)) return 0.0;
            if ((first.Length == 0) || (first.Length == 0)) return 0.0;
            if (first == second) return 1.0;

            int stepsToSame = LevenshteinDistance(first, second);
            return 1.0 - ((double)stepsToSame / (double)Math.Max(first.Length, second.Length));
        }

        private double Compare(ESGame esGame, Game game, string romName)
        {
            double max = 0;
            max = Math.Max(max, Similarity(game.Name, esGame.Name));
            max = Math.Max(max, Similarity(Path.GetFileNameWithoutExtension(romName), Path.GetFileNameWithoutExtension(esGame.Path)));
            max = Math.Max(max, Similarity(game.Name, Path.GetFileNameWithoutExtension(esGame.Path)));
            return max;
        }

        private ESGame FindGame(Game game)
        {
            string romName = Path.GetFileName(GetRomPath(game) ?? string.Empty);

            List<Tuple<ESGame, double>> similarity = new List<Tuple<ESGame, double>>();

            var mostSimilar =
                ESGames.Select(s => new Tuple<ESGame, double>(s, Compare(s, game, romName)))
                .Where(t => t.Item2 > 0.75)
                .OrderByDescending(d => d.Item2)
                .Select(d => d.Item1)
                .ToList();


            if (mostSimilar.Count == 0)
                return new ESGame();

            if (mostSimilar.Count == 1)
                return mostSimilar.First();

            esGame = new ESGame();

            foreach( PropertyInfo prop in typeof(ESGame).GetProperties() )
            {
                List<string> vals = mostSimilar.Select(d => prop.GetValue(d) as string).Where(s=>!string.IsNullOrEmpty(s)).ToList();
                if (vals.Count == 0) continue;

                string res = vals.First();
                if (prop.GetCustomAttribute<PathAttribute>() == null)
                {
                    foreach (var v in vals)
                    {
                        if ((res.Contains('_') && !v.Contains('_'))
                        || v.Length > res.Length && (!v.Contains('_') || res.Contains('_') || res == ""))
                            res = v;
                    }
                }
                prop.SetValue(esGame, res);
            }

            return esGame;
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

        private MetadataFile ToMetadataFile(ESGame game, List<string>priority, int maxWidth = int.MaxValue, int maxHeight = int.MaxValue)
        {
            foreach(string field in priority)
            {
                MetadataFile res = ToMetadataFile(game.Get(field), maxWidth, maxHeight);
                if (res != default) return res;
            }
            return default;
        }

        private void AddLink(string name, string path, List<Link> links)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                links.Add(new Link(name, path));
        }

        public override string GetName(GetMetadataFieldArgs args) => esGame.Name;

        public override string GetDescription(GetMetadataFieldArgs args) => esGame.Desc;

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        => ToMetadataFile(esGame, new List<string>(){"Thumbnail", "Image"}, 484, 680);

        public override MetadataFile GetIcon(GetMetadataFieldArgs args)
        => ToMetadataFile(esGame, new List<string>(){"Marquee", "Thumbnail", "Image"}, 200, 200);

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        => ToMetadataFile(esGame, new List<string>(){"Fanart", "Image"}, 1920, 1080);

        public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
        => ToMetadataProperty(esGame.Genre, '/');

        public override IEnumerable<MetadataProperty> GetRegions(GetMetadataFieldArgs args)
        => ToMetadataProperty(esGame.Region?.ToUpper(), '/');

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            if (!string.IsNullOrEmpty(esGame.ReleaseDate)
                && DateTime.TryParseExact(esGame.ReleaseDate, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
            )
            {
                return new ReleaseDate(date);
            }
            return base.GetReleaseDate(args);
        }

        public override int? GetCriticScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esGame.Rating);

        public override int? GetCommunityScore(GetMetadataFieldArgs args)
        => ToMetadataScore(esGame.Rating);

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esGame.Developer, '/');

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        => ToMetadataProperty(esGame.Publisher, '/');

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            List<Link> links = new List<Link>();
            AddLink("VideoTrailer", GetVideoTrailer(), links);
            AddLink("Logo", GetLogo(), links);
            AddLink("Bezel", GetBezel(), links);
            AddLink("Fanart", GetFanart(), links);

            Options.GameData.Links.ForEach(l => {if (!links.Select(li => li.Name).Contains(l.Name)) links.Add(l); });

            LinksRequested = links.Count > 0;

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

        public string GetLogo() => esGame.Marquee;
        public string GetVideoTrailer() => esGame.Video;
        public string GetBezel() => esGame.Bezel;
        public string GetFanart() => esGame.Fanart;
        public string GetFavorite() => esGame.Favorite;


        private void CopyExtraMetadata(Game game, string path, string destination)
        {
            if (
                string.IsNullOrEmpty(path)
            || !File.Exists(path)
            || 0 != string.Compare(Path.GetExtension(path), Path.GetExtension(destination), StringComparison.OrdinalIgnoreCase)
            ) return;

            string destPath = Path.Combine(PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "Games", game.Id.ToString(), destination);
            bool overwrite = false;
            if (File.Exists(destPath) && !overwrite) return;

            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            File.Copy(path, destPath, overwrite);
        }

        public override void Dispose()
        {
            if ( Options.IsBackgroundDownload
              && LinksRequested
              && Settings.CopyExtraMetadataOnLinks
              && Settings.Overwrite
            )
            {
                CopyExtraMetadata(Options.GameData, GetVideoTrailer(), "VideoTrailer.mp4");
                CopyExtraMetadata(Options.GameData, GetLogo(), "Logo.png");
                CopyExtraMetadata(Options.GameData, GetBezel(), "Bezel.png");
                CopyExtraMetadata(Options.GameData, GetFanart(), "Fanart.png");
            }

            if ( Options.IsBackgroundDownload
              && Settings.ImportFavorite
              && TagsRequested )
            {
                Game game = PlayniteApi.Database.Games.FirstOrDefault(g => g.Id == Options.GameData.Id);
                if (game != default
                    && 0 == string.Compare(GetFavorite(), "true", StringComparison.OrdinalIgnoreCase)
                    && !game.Favorite)
                {
                    game.Favorite = true;
                    PlayniteApi.Database.Games.Update(game);
                }
            }
        }
    }
}