using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace DrMangle
{
    public class GameData
    {
        public string GameName { get; set; }
        public int GameDataId { get; set; }
        public ParkManager CurrentLevel { get; set; }
        public ArenaBattleCalculator Arena { get; set; }
        public int CurrentRegion { get; set; }
        public string RegionText
        {
            get
            {
                return CurrentLevel.Locations[CurrentRegion].ParkName;
            }
        }
        public PlayerData CurrentPlayer { get; set; }
        public PlayerData[] AiPlayers { get; set; }
        public List<MonsterGhost> Graveyard { get; set; }
        public int GameDayNumber { get; set; }

        [JsonConstructor]
        public GameData() { }

        public GameData(string name, int aiCount, int gameID, Random RNG)
        {
            GameDataId = gameID;

            GameName = name;

            AiPlayers = new PlayerData[aiCount];
            GenerateAI(AiPlayers);

            CurrentLevel = new ParkManager(RNG, AiPlayers.Length + 1);
            CurrentPlayer = new PlayerData(name, false);
            Graveyard = new List<MonsterGhost>();

            CurrentRegion = 0; //at the lab
            GameDayNumber = 0;
        }

        private void GenerateAI(PlayerData[] ai)
        {
            for (int i = 0; i < ai.Length; i++)
            {
                ai[i] = new PlayerData("rando", true);
            }
        }

       

        public void MoveRegions()
        {
            int intInput;
            Console.WriteLine("You are currently in the " + RegionText + ",");

            Console.WriteLine("what will you do next?");
            for (int i = 1; i < 5; i++)
            {
                if (CurrentRegion == i)
                {
                    Console.WriteLine(i + " - Stay in the " + CurrentLevel.Locations[i].ParkName);
                }
                else
                {
                    Console.WriteLine(i + " - Go to the " + CurrentLevel.Locations[i].ParkName);
                }
            }

            // intInput = StaticUtility.CheckInput(1, 4);
            // CurrentRegion = intInput;
        }

    }
}

    