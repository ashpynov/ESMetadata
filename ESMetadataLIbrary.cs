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



        private List<ESGame> ESGames = new List<ESGame>();
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

            gameData.AddGame(FindGame(game));

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

        private void LoadLibrary(Game game)
        {
            // find gamelist.xml
            // take rom path and find closest
            // game not in list => search gamelists in "library"

            List<string> romsPath = GetRomsPath(game);

            if (romsPath is null)
                return;

            List<string> gameLists = new List<string>();


            foreach (string romPath in romsPath)
            {
                string gamelistFile = Path.Combine(Path.GetDirectoryName(romPath), "gamelist.xml");

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
        private ESGame FindGame(Game game)
        {
            string romName = Path.GetFileName(GetRomsPath(game).FirstOrDefault() ?? string.Empty);

            List<Tuple<ESGame, double>> similarity = new List<Tuple<ESGame, double>>();

            var mostSimilar =
                ESGames.Select(s => new Tuple<ESGame, double>(s, Compare(s, game, romName)))
                .Where(t => t.Item2 > 0.75)
                .OrderByDescending(d => d.Item2)
                .Select(d => d.Item1)
                .ToList();


            if (mostSimilar.Count == 0)
                return new ESGame();

            if (mostSimilar.Count == 1 || !Settings.BestMatchWithDesc)
                return mostSimilar.First();


            ESGame esGame = new ESGame().Extend(mostSimilar.First());

            ESGame withDesc = mostSimilar.FirstOrDefault(g => !string.IsNullOrEmpty(g.Desc));

            if (withDesc == null)
            {
                esGame.Name = null;
            }
            else if (0!=string.Compare(withDesc.Path, esGame.Path, StringComparison.OrdinalIgnoreCase))
            {
                esGame.Name = withDesc.Name;
                esGame.Extend(withDesc);
            }

            return esGame;
        }

        private double Compare(ESGame esGame, Game game, string romName)
        {
            double max = 0;
            max = Math.Max(max, Tools.Similarity(game.Name, esGame.Name));
            max = Math.Max(max, Tools.Similarity(Path.GetFileNameWithoutExtension(romName), Path.GetFileNameWithoutExtension(esGame.Path)));
            max = Math.Max(max, Tools.Similarity(game.Name, Path.GetFileNameWithoutExtension(esGame.Path)));
            return max;
        }
    }

};
