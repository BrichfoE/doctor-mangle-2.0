using doctor_mangle.constants;
using doctor_mangle.models.parts;
using System.Collections.Generic;

namespace doctor_mangle_data.models
{
    public class ParkData
    {
        private LinkedList<BodyPart> _partsList = new LinkedList<BodyPart>();
        // private stores for serilaization
        internal List<Head> _heads = new List<Head>();
        internal List<Torso> _torsos = new List<Torso>();
        internal List<Arm> _arms = new List<Arm>();
        internal List<Leg> _legs = new List<Leg>();
        public string ParkName { get; set; }
        public Structure ParkPart { get; set; }
        public LinkedList<BodyPart> PartsList { get => _partsList; }
    }
}
