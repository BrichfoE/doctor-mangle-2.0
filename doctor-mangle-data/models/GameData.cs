using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.Service;
using doctor_mangle.services;
using doctor_mangle_data.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace doctor_mangle
{
    public class GameData
    {
        public string GameName { get; set; }
        public int GameDataId { get; set; }
        public ParkData[] Parks { get; set; }
        public ArenaBattleCalculator Arena { get; set; }
        public int CurrentRegion { get; set; }
        public string RegionText
        {
            get
            {
                return Parks[CurrentRegion].ParkName;
            }
        }
        public PlayerData CurrentPlayer { get; set; }
        public PlayerService _playerService { get; set; }
        public PlayerData[] AiPlayers { get; set; }
        public List<MonsterGhost> Graveyard { get; set; }
        public int GameDayNumber { get; set; }

        [JsonConstructor]
        public GameData() { }

        // todo: seems like a prime target for either an abstract factory or builder pattern
        public GameData(string name, int aiCount, int gameID, Random RNG)
        {
            _playerService = new PlayerService();
            GameDataId = gameID;

            GameName = name;

            AiPlayers = new PlayerData[aiCount];
            GenerateAI(AiPlayers);

            var _parkService = new ParkService();
            Parks = _parkService.GenerateParks();
            Parks = _parkService.AddParts(Parks, RNG, AiPlayers.Length + 1);
            CurrentPlayer = _playerService.GeneratePlayer(name, false);
            Graveyard = new List<MonsterGhost>();

            CurrentRegion = 0; //at the lab
            GameDayNumber = 0;
        }

        private void GenerateAI(PlayerData[] ai)
        {
            for (int i = 0; i < ai.Length; i++)
            {
                ai[i] = _playerService.GeneratePlayer("rando", true);
            }
        }

        // todo: this should actually probably move to the GameService once it's refactored
        public string PrintRegionOptions()
        {
            string result = string.Empty;
            for (int i = 1; i < 5; i++)
            {
                if (CurrentRegion == i)
                {
                    result += $"{i} - Stay in the {Parks[i].ParkName} \r\n";
                }
                else
                {
                    result += $"{i} - Go to the {Parks[i].ParkName} \r\n";
                }
            }
            return result;
        }

    }
}

    