using doctor_mangle.interfaces;
using doctor_mangle.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace doctor_mangle_console_app
{
    public class GameRepo : IGameRepository
    {
        private readonly string filePath = ConfigurationManager.AppSettings["filepath"];
        private readonly IParkService _parkService;
        private readonly IMonsterService _monsterService;
        private int exceptionCount;
        private Dictionary<string, int> gameIndex = new Dictionary<string, int>();

        public GameRepo(IParkService parkService, IMonsterService monsterService)
        {
            this._parkService = parkService;
            this._monsterService = monsterService;
        }

        public void FileSetup()
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            if (!Directory.Exists($"{filePath}\\Errors"))
            {
                Directory.CreateDirectory(filePath+"\\Errors");
            }

            string indexFile = Path.Combine(filePath, "Index.txt");
            if (!File.Exists(indexFile))
            {
                var file = File.Create(indexFile);
                file.Dispose();
                this.gameIndex = new Dictionary<string, int>();
                this.gameIndex.Add("_placeholder", 0);
                File.WriteAllText(Path.Combine(filePath, "Index.txt"), JsonConvert.SerializeObject(gameIndex, Formatting.Indented));
            }
            else
            {
                var text = File.ReadAllText(indexFile);
                this.gameIndex = JsonConvert.DeserializeObject<Dictionary<string, int>>(text);
            }
        }

        public void SaveGame(GameData gd)
        {
            foreach (var park in gd.Parks)
            {
                _parkService.MovePartsForSerilaization(park);
            }
            _monsterService.MovePartsForSerilaization(gd.CurrentPlayer.Monster);
            foreach (var player in gd.AiPlayers)
            {
                _monsterService.MovePartsForSerilaization(player.Monster);
            }

            string saveFile = Path.Combine(filePath, $"dat_{gd.GameDataId}.txt");
            if (!File.Exists(saveFile))
            {
                var gameFile = File.Create(saveFile);
                gameFile.Dispose();
                if (gameIndex == null)
                {
                    gameIndex = new Dictionary<string, int>() { };
                }
                if (!gameIndex.ContainsKey(gd.GameName))
                {
                    gameIndex.Add(gd.GameName, gd.GameDataId);
                    File.WriteAllText(Path.Combine(filePath, "Index.txt"), JsonConvert.SerializeObject(gameIndex, Formatting.Indented));
                }
            }

            File.WriteAllText(saveFile, JsonConvert.SerializeObject(gd, Formatting.Indented));
            Console.WriteLine("Game Saved");
            this.MovePartsAfterDeserialization(gd);
        }

        public bool CanLoadGames()
        {
            // check if there's a placeholder in the index that would skew results
            var expected = gameIndex["_placeholder"] == 0 ? 1 : 0;
            return gameIndex.Count > expected;
        }

        public GameData LoadGame()
        {
            GameData data = null;
            int gameId = 1;
            int intInput;

            Console.WriteLine("Would you like to load a previous game?");
            Console.WriteLine("0 - Start New Game");
            Console.WriteLine("1 - Load a Previous Game");
            intInput = StaticConsoleHelper.CheckInput(0, 1);
            if (intInput == 0)
            {
                return null;
            }
            bool halt = true;
            while (halt)
            {
                Console.WriteLine("Please enter the name of the game you would like to load.");
                foreach (var game in gameIndex)
                {
                    if (game.Key != "_placeholder" && game.Value != 0)
                    {
                        Console.WriteLine(game.Value + " - " + game.Key);
                    }
                }
                string gameName = Console.ReadLine();

                if (gameIndex.TryGetValue(gameName, out gameId))
                {
                    halt = false;
                }
                else
                {
                    Console.WriteLine("Invalid game name, please enter the name of a game.");
                }
            }
            string saveFile = Path.Combine(filePath, "dat_" + gameId.ToString() + ".txt");

            if (File.Exists(saveFile))
            {
                string fileText = File.ReadAllText(saveFile);
                data = JsonConvert.DeserializeObject<GameData>(fileText);
                Console.WriteLine("Load successful!");
            }
            this.MovePartsAfterDeserialization(data);
            return data;
        }

        public int GetNextGameID()
        {
            int GameID;

            if (gameIndex == null)
            {
                GameID = 1;
            }
            else
            {
                GameID = gameIndex.Last().Value + 1;
            }

            return GameID;
        }

        public int GetGameIdFromName(string name)
        {
            return gameIndex.ContainsKey(name)
                ? gameIndex[name]
                : -1;
        }

        public void LogException(GameData gd, string exceptionText, Exception ex, bool willClose)
        {
            exceptionCount += 1;
            string errorFileName = Path.Combine(filePath, "Errors\\dat_" + gd.GameDataId.ToString() + "_Exception" + exceptionCount.ToString() +"_"+ DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
            if (!File.Exists(errorFileName))
            {
                var errorFile = File.Create(errorFileName);
                errorFile.Dispose();
            }

            var sw = File.AppendText(errorFileName);
            sw.WriteLine(exceptionText);
            sw.WriteLine("Message: " + ex.Message);
            sw.WriteLine("HelpLink: " + ex.HelpLink);
            sw.WriteLine("Data: " + ex.Data);
            sw.WriteLine("Source: " + ex.Source);
            sw.WriteLine("StackTrace: " + ex.StackTrace);
            sw.WriteLine("InnerException: " + ex.InnerException);
            sw.WriteLine(JsonConvert.SerializeObject(gd, Formatting.Indented));

            if (willClose)
                StaticConsoleHelper.TalkPause("Something has gone wrong, the game will now close and unsaved progress will be lost.");
            else
                StaticConsoleHelper.TalkPause("Error Logged, section skipped.");
        }

        private void MovePartsAfterDeserialization(GameData gd)
        {
            foreach (var park in gd.Parks)
            {
                _parkService.MovePartsAfterDeserilaization(park);
            }
            _monsterService.MovePartsAfterDeserilaization(gd.CurrentPlayer.Monster);
            foreach (var player in gd.AiPlayers)
            {
                _monsterService.MovePartsAfterDeserilaization(player.Monster);
            }
        }
    }
}
