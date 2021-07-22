using doctor_mangle.models.parts;

namespace doctor_mangle.models.monsters
{
    public abstract class MonsterBase : PartsCollection
    {
        public int Wins { get; set; }
        public int Fights { get; set; }
        public string Name { get; set; }
    }
}
