using doctor_mangle.constants;
using doctor_mangle.models.parts;
using System.Collections.Generic;

namespace doctor_mangle.models
{
    public class PlayerData
    {
        private BodyPart[] _bag = new BodyPart[5];
        private List<BodyPart> _workshop = new List<BodyPart>();
        private Dictionary<Structure, int> _structures = new Dictionary<Structure, int>(){
            { Structure.Magical, 0 },
            { Structure.Human, 0 },
            { Structure.Animal, 0 },
            { Structure.Mechanical, 0 },
            { Structure.Rock, 0 }
        };

        public string Name { get; set; }
        public bool IsAI { get; set; }
        public int WinsCount { get; set; }
        public int FightsCount { get; set; }
        public int Luck { get; set; }
        public decimal Money { get; set; }
        public MonsterData Monster { get; set; }
        public BodyPart[] Bag { get => _bag; }
        public List<BodyPart> WorkshopCuppoard { get => _workshop; set => _workshop = value;  }
        public Dictionary<Structure, int> SpareParts { get => _structures; }
    }
}
