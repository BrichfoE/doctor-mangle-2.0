using doctor_mangle.constants;
using doctor_mangle.models.monsters;
using doctor_mangle_data.models;
using System.Collections.Generic;

namespace doctor_mangle.models
{
    public class GameData
    {
        public int GameDataId { get; set; }
        public string GameName { get; set; }
        public ParkData[] Parks { get; set; }
        public int CurrentRegion { get; set; }
        public string RegionText
        {
            get
            {
                return Parks[CurrentRegion].ParkName;
            }
        }
        public PlayerData CurrentPlayer { get; set; }
        public PlayerData[] AiPlayers { get; set; }
        public List<MonsterGhost> Graveyard { get; set; }
        public int GameDayNumber { get; set; }
        public Phase GamePhase { get; set; }
    }
}

    