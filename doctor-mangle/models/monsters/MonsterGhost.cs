using Newtonsoft.Json;

namespace doctor_mangle.models.monsters
{
    public class MonsterGhost : MonsterBase
    {
        public int DeathDay { get; set; }

        [JsonConstructor]
        public MonsterGhost() { }

        // todo: move this to MonsterService when it exists
        public MonsterGhost(MonsterData deceased, int day)
        {
            Name = deceased.Name;
            Wins = deceased.Wins;
            Fights = deceased.Fights;
            DeathDay = day;
        }
    }
}
