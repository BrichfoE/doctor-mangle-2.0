using doctor_mangle.models.monsters;
using System.Collections.Generic;

namespace doctor_mangle.models.battles
{
    public class BattleResult
    {
        private List<string> _text = new List<string>();
        private List<BattleRound> _rounds = new List<BattleRound>();

        public PlayerData BlueCorner { get; set; }
        public MonsterData Blue { get => BlueCorner.Monster; }
        public PlayerData GreenCorner { get; set; }
        public MonsterData Green { get => GreenCorner.Monster; }
        public bool? WinnerIsBlue { get; set; }
        public PlayerData Winner
        {
            get
            {
                return WinnerIsBlue != null
                    ? WinnerIsBlue.Value
                        ? BlueCorner
                        : GreenCorner
                    : null;
            }
        }
        public List<string> Text { get => _text; }
        public List<BattleRound> Rounds { get => _rounds; }
    }
}
