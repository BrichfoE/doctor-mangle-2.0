using System.Collections.Generic;

namespace doctor_mangle.models.battles
{
    public class ArenaResult
    {
        public string OpeningLine { get; set; }
        public string ClosingLine { get; set; }
        public List<BattleResult> Fights { get; set; }
    }
}
