using doctor_mangle.constants;
using doctor_mangle.models.parts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace doctor_mangle.models.monsters
{
    public class MonsterData : MonsterBase
    {
        private List<BodyPart> _parts = new List<BodyPart>();
        private Dictionary<Stat, float> _stats = new Dictionary<Stat, float>
            {
                { Stat.Alacrity, 1f },
                { Stat.Strength, 1f },
                { Stat.Endurance, 1f },
                { Stat.Technique, 1f }
            };
        // private stores for serilaization
        internal List<Head> _heads;
        internal List<Torso> _torsos;
        internal List<Arm> _arms;
        internal List<Leg> _legs;
        public List<BodyPart> Parts => _parts;
        public List<BodyPart> Heads => _parts.Where(x => x.GetType() == typeof(Head)).ToList();
        public List<BodyPart> Torsos => _parts.Where(x => x.GetType() == typeof(Torso)).ToList();
        public Dictionary<Stat, float> MonsterStats { get
            {
                CalculateStats();
                return _stats;
            }
            private set
            {
                MonsterStats = _stats;
            }
        }
        public bool CanFight { get
            {
                return CanFightNow();
            }
        }

        [JsonConstructor]
        public MonsterData() { }

        public MonsterData(string newName, IEnumerable<BodyPart> newParts)
        {
            Name = newName;
            foreach (var part in newParts)
            {
                this.Parts.Add(part);
            }
        }

        // todo: the durability adjustment here should really be in the PartService and get called by the battle calculator
        // todo: updating the durability should also probably be in a MonsterService
            // monster service things to do eventually
               //display monster
               //add/remove parts
               //activate/scrap
               //convert to ghost
               //repair
        private void CalculateStats()
        {
            _stats[Stat.Alacrity] = 0;
            _stats[Stat.Strength] = 0;
            _stats[Stat.Endurance] = 0;
            _stats[Stat.Technique] = 0;

            foreach (var part in this.Parts)
            {
                _stats[Stat.Alacrity] += part.PartStats[Stat.Alacrity] * (float)part.PartDurability;
                _stats[Stat.Strength] += part.PartStats[Stat.Strength] * (float)part.PartDurability;
                _stats[Stat.Endurance] += part.PartStats[Stat.Endurance] * (float)part.PartDurability;
                _stats[Stat.Technique] += part.PartStats[Stat.Technique] * (float)part.PartDurability;
            }
        }

        private bool CanFightNow()
        {
            bool head = this.Parts.Any(x => x.PartType == Part.head && x.PartDurability > 0);
            bool torso = this.Parts.Any(x => x.PartType == Part.torso && x.PartDurability > 0);
            bool limb = this.Parts.Any(x => x.PartType != Part.head
                && x.PartType != Part.torso
                && x.PartDurability > 0);

            return head && torso && limb;
        }
    }
}
