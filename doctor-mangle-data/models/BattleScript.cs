using doctor_mangle.models.monsters;
using System.Collections.Generic;

namespace doctor_mangle.models
{
    public class BattleScript
    {
        private List<string> _text = new List<string>();
        private List<string> _debug = new List<string>();

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
        public List<string> Debug { get => _debug; }
    }
}
