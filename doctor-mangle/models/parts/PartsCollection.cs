using System.Collections.Generic;

namespace doctor_mangle.models.parts
{
    public class PartsCollection
    {
        // private stores for serilaization
        public List<Head> _heads = new List<Head>();
        public List<Torso> _torsos = new List<Torso>();
        public List<Arm> _arms = new List<Arm>();
        public List<Leg> _legs = new List<Leg>();
    }
}
