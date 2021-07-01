using doctor_mangle.constants;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace doctor_mangle.models.parts
{
    public abstract class BodyPart 
    {
        private Dictionary<Stat, float> _stats = new Dictionary<Stat, float>
            {
                { Stat.Alacrity, 1f },
                { Stat.Strength, 1f },
                { Stat.Endurance, 1f },
                { Stat.Technique, 1f }
            };

        public virtual string PartName
        {
            get => $"{PartRarity} {PartStructure} {PartType}";
        }
        
        public virtual Part PartType { get; set; }
        public Structure PartStructure { get; set; }
        public Rarity PartRarity { get; set; }
        public Dictionary<Stat, float> PartStats { get => _stats; }

        public float[] Stats { get; set; }

        public decimal PartDurability { get; set; }

        [JsonConstructor]
        public BodyPart() //empty constructor
        {
            Stats = new float[4];
        }
    }    
}
