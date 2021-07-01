using doctor_mangle.constants;
using doctor_mangle.models.parts;
using System.Collections.Generic;

namespace DrMangle.Model
{
    public class ParkData
    {
        public string ParkName { get; set; }
        public Structure ParkPart { get; set; }
        public LinkedList<BodyPart> PartsList { get; set; }
    
        public ParkData(string name, int part)
        {
            ParkName = name;
            ParkPart = (Structure)part;
            PartsList = new LinkedList<BodyPart>();
        }
    }
}
