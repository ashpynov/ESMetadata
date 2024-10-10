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
using System.Collections.ObjectModel;
using System.Windows.Documents;
using ESMetadata.Common;

namespace ESMetadata
{
    public class ESMetadataLibrary: IDisposable
    {



        static private List<ESGame> ESGames = new List<ESGame>();
        static private int loadedESGamesHash = 0;
        static private DateTime loadedESGamesLastUsed = default;


        private readonly IPlayniteAPI PlayniteApi;
        private readonly ESMetadataSettings Settings;

        private ESGameOptions gameData;

        public ESGameOptions GameData { get => gameData; }


        public void Dispose()
        {}

        public ESMetadataLibrary(ESMetadata plugin, Game game)
        {
            PlayniteApi = plugin.PlayniteApi;
            Settings = plugin.GetSettings();
            LoadLibrary(game);
            gameData = new ESGameOptions(plugin.GetSettings());

            List<ESGame> similarGames = FindGames(game);

            gameData.AddGame(similarGames.FirstOrDefault());
            if(Settings.BestMatchWithDesc && string.IsNullOrEmpty(gameData.GetField(MetadataField.Description)))
            {
                 gameData.AddGame(similarGames.FirstOrDefault(g => !string.IsNullOrEmpty(g.Desc)));
            }
        }

        public List<ESGame> Games { get => ESGames; }

        private List<string> GetRomsPath(Game game)
        {
            if (game.Roms?.FirstOrDefault() is null )
                return default;

            Guid emulatorId = game.GameActions.FirstOrDefault(a => a.IsPlayAction && a.Type == GameActionType.Emulator)?.EmulatorId ?? default;
            string emulatorPath = PlayniteApi.Database.Emulators.Get(emulatorId)?.InstallDir ?? "";
            return game.Roms.Select(rom => PlayniteApi.ExpandGameVariables(game, rom.Path, emulatorPath).Replace("\\\\", "\\")).ToList();
        }

        private static int GetHashCodeOfList<T>(IEnumerable<T> list)
        {
            List<int> codes = new List<int>();
            foreach (T item in list)
            {
                codes.Add(item.GetHashCode());
            }
            codes.Sort();
            int hash = 0;
            foreach (int code in codes)
            {
                unchecked
                {
                    hash *= 251; // multiply by a prime number
                    hash += code; // add next hash code
                }
            }
            return hash;
        }

        private void LoadLibrary(Game game)
        {
            // find gamelist.xml
            // take rom path and find closest
            // game not in list => search gamelists in "library"

            List<string> romsPath = GetRomsPath(game)?.Select(p=>Path.GetDirectoryName(p))?.ToList();

            if (romsPath is null)
                return;

            List<string> gameLists = new List<string>();

            foreach (string romPath in romsPath)
            {
                string gamelistFile = Path.Combine(romPath, "gamelist.xml");

                while (!File.Exists(gamelistFile) && !string.IsNullOrEmpty(gamelistFile))
                {
                    gamelistFile =
                        Path.GetDirectoryName(gamelistFile) != Path.GetPathRoot(gamelistFile)
                        ? Path.Combine(Directory.GetParent(Path.GetDirectoryName(gamelistFile)).FullName, Path.GetFileName(gamelistFile))
                        : default;
                }

                if (string.IsNullOrEmpty(gamelistFile))
                {

                }
                else
                {
                    gameLists.AddMissing(gamelistFile);
                }
            }



            int hash = GetHashCodeOfList(gameLists);

            if ( hash == loadedESGamesHash && (DateTime.Now - loadedESGamesLastUsed) < TimeSpan.FromSeconds(3))
            {
                loadedESGamesLastUsed = DateTime.Now;
                return;
            }

            loadedESGamesLastUsed = DateTime.Now;
            loadedESGamesHash = hash;

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

        private List<ESGame> FindGames(Game game)
        {

            string romPath = GetRomsPath(game)?.FirstOrDefault() ?? default;

            if (romPath == default) return new List<ESGame>();

            List<ESGame> byPath = ESGames.Where(es => Tools.Equal(es.Path, romPath)).ToList();

            return (byPath.Count > 0 && (!Settings.BestMatchWithDesc || !string.IsNullOrEmpty(byPath.First().Desc)))
                ? byPath
                : ESGames.Select(s => new Tuple<ESGame, double>(s, Compare(s, game, romPath, 0.75)))
                    .Where(t => t.Item2 > 0.75)
                    .OrderByDescending(d => d.Item2)
                    .Select(d => d.Item1)
                    .ToList();
        }

        private double Compare(ESGame esGame, Game game, string romPath, double minSimilarity )
        {

            double max = 0;
            bool ignoreArticles = Settings.IgnoreArticles;

            string gameName = Tools.DeConventGameName(game.Name.ToLower(), ignoreArticles);

            string romPathName = Tools.DeConventGameName(Path.GetFileNameWithoutExtension(romPath), ignoreArticles);
            string romName = game.Roms?.FirstOrDefault()?.Name;
            romName = !string.IsNullOrEmpty(romName) ? Tools.DeConventGameName(romName, ignoreArticles) : romName;

            string esGameName = Tools.DeConventGameName(esGame.Name.ToLower(), ignoreArticles);
            string esGamePath = Tools.DeConventGameName(Path.GetFileNameWithoutExtension(esGame.Path), ignoreArticles);

            max = Math.Max(max, Tools.Equal(romPath, esGame.Path) ? 1.1 : 0); //Force path match priority
            max = Math.Max(max, Tools.Similarity(romName, esGameName, minSimilarity));
            max = Math.Max(max, Tools.Similarity(gameName, esGameName, minSimilarity));
            max = Math.Max(max, Tools.Similarity(romPathName, esGamePath, minSimilarity));
            max = Math.Max(max, Tools.Similarity(gameName, esGamePath, minSimilarity));

            return max;
        }
    }

};
