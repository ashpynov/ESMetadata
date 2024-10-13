using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ESMetadata.Settings;
using ESMetadata.Tools;
using ESMetadata.Models.Gamelist;
using ESMetadata.Models.ESGame;
using ESMetadata.Extensions;

namespace ESMetadata
{
    public class ESMetadataLibrary: IDisposable
    {
        static private List<GamelistGame> GamelistGames = new List<GamelistGame>();
        static private int loadedGamelistHash = 0;
        static private DateTime loadedGamelistLUT = default;

        private IPlayniteAPI PlayniteApi { get => ESMetadata.PlayniteApi; }
        private readonly ESMetadataSettings Settings;

        private readonly bool IsBackgroundDownload;
        private readonly double SimilarityEdge = 0.75;

        private ESGame gameData;

        public ESGame Game { get => gameData; }

        public void Dispose()
        {}


        public ESMetadataLibrary(ESMetadata plugin, Game game, bool isBackgroundDownload)
        {
            Settings = plugin.GetSettings();
            IsBackgroundDownload = isBackgroundDownload;

            LoadLibrary(game);
            gameData = new ESGame(Settings);

            LoadBestMatchedGame(game);
        }

        private void LoadSameRateGame(ref List<Tuple<GamelistGame, double>> similarGames, Tuple<GamelistGame, double> startFrom)
        {
            if (similarGames.Count == 0) return;
            if (startFrom is null) return;

            gameData.AddGameInfo(similarGames.Pop(startFrom)?.Item1);
            List<Tuple<GamelistGame, double>> gamesToAdd = similarGames.Where(gs => gs.Item2 == startFrom.Item2).ToList();

            foreach( Tuple<GamelistGame, double> gs in gamesToAdd)
            {
                gameData.AddGameInfo(similarGames.Pop(gs).Item1);
            }
        }

        private void LoadBestMatchedGame( Game game )
        {
            List<Tuple<GamelistGame,double>> similarGames = FindGames(game);

            LoadSameRateGame(ref similarGames, similarGames.First());

            if(Settings.BestMatchWithDesc && gameData.GetField(MetadataField.Description).IsNullOrEmpty())
            {
                LoadSameRateGame(ref similarGames, similarGames.FirstOrDefault(g => !g.Item1.Desc.IsNullOrEmpty()));
            }

            if (!IsBackgroundDownload && Settings.NonStrictMediaSuggest)
            {
                foreach (Tuple<GamelistGame, double> g in similarGames)
                {
                    gameData.AddGameInfo(g.Item1, ImagesOnly: true);
                }
            }
        }

        public List<GamelistGame> Games { get => GamelistGames; }

        private List<string> GetRomsPath(Game game)
        {
            if (game.Roms?.FirstOrDefault() is null )
                return default;

            Guid emulatorId = game.GameActions.FirstOrDefault(a => a.IsPlayAction && a.Type == GameActionType.Emulator)?.EmulatorId ?? default;
            string emulatorPath = PlayniteApi.Database.Emulators.Get(emulatorId)?.InstallDir ?? "";
            return game.Roms.Select(rom => PlayniteApi.ExpandGameVariables(game, rom.Path, emulatorPath).Replace("\\\\", "\\")).ToList();
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

                while (!File.Exists(gamelistFile) && !gamelistFile.IsNullOrEmpty())
                {
                    gamelistFile =
                        Path.GetDirectoryName(gamelistFile) != Path.GetPathRoot(gamelistFile)
                        ? Path.Combine(Directory.GetParent(Path.GetDirectoryName(gamelistFile)).FullName, Path.GetFileName(gamelistFile))
                        : default;
                }

                if (gamelistFile.IsNullOrEmpty())
                {

                }
                else
                {
                    gameLists.AddMissing(gamelistFile);
                }
            }



            int hash = gameLists.GetUnorderedHashCode();

            if ( hash == loadedGamelistHash && (DateTime.Now - loadedGamelistLUT) < TimeSpan.FromSeconds(3))
            {
                loadedGamelistLUT = DateTime.Now;
                return;
            }

            loadedGamelistLUT = DateTime.Now;
            loadedGamelistHash = hash;

            GamelistGames = new List<GamelistGame>();
            foreach(string file in gameLists)
            {
                XDocument doc = XDocument.Load(file);

                string root = Path.GetDirectoryName(file);

                foreach (var gameElement in doc.Descendants("game"))
                {
                    GamelistGames.Add(new GamelistGame(gameElement, root));
                }
            }
        }

        private List<Tuple<GamelistGame,double>> FindGames(Game game)
        {
            string romPath = GetRomsPath(game)?.FirstOrDefault() ?? default;

            if (romPath == default) return new List<Tuple<GamelistGame,double>>();

            if (IsBackgroundDownload)
            {
                List<Tuple<GamelistGame,double>> byPath = GamelistGames
                    .Where(es => es.Path.Equal(romPath))
                    .Select(es => new Tuple<GamelistGame, double>(es, 1.0))
                    .ToList();
                if (byPath.Count > 0 && (!Settings.BestMatchWithDesc || !byPath.First().Item1.Desc.IsNullOrEmpty()))
                {
                    return byPath;
                }
            }

            return GamelistGames.Select(s => new Tuple<GamelistGame, double>(s, Compare(s, game, romPath, SimilarityEdge)))
                    .Where(t => t.Item2 > SimilarityEdge)
                    .OrderByDescending(d => d.Item2)
                    .ToList();
        }

        private double Compare(GamelistGame esGame, Game game, string romPath, double minSimilarity )
        {

            double max = 0;
            bool ignoreArticles = Settings.IgnoreArticles;

            string gameName = Fuzzy.SimplifyName(game.Name.ToLower(), ignoreArticles);

            string romPathName = Fuzzy.SimplifyName(Path.GetFileNameWithoutExtension(romPath), ignoreArticles);
            string romName = game.Roms?.FirstOrDefault()?.Name;
            romName = !romName.IsNullOrEmpty() ? Fuzzy.SimplifyName(romName, ignoreArticles) : romName;

            string esGameName = Fuzzy.SimplifyName(esGame.Name.ToLower(), ignoreArticles);
            string esGamePath = Fuzzy.SimplifyName(Path.GetFileNameWithoutExtension(esGame.Path), ignoreArticles);

            max = Math.Max(max, romPath.Equal(esGame.Path) ? 1.1 : 0); //Force path match priority
            max = Math.Max(max, Fuzzy.Similarity(romName, esGameName, minSimilarity));
            max = Math.Max(max, Fuzzy.Similarity(gameName, esGameName, minSimilarity));
            max = Math.Max(max, Fuzzy.Similarity(romPathName, esGamePath, minSimilarity));
            max = Math.Max(max, Fuzzy.Similarity(gameName, esGamePath, minSimilarity));

            return max;
        }
    }

};
