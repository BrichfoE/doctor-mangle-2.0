using doctor_mangle.constants;

namespace doctor_mangle.models.parts
{
    public class Leg : PairedBodyPart
    {
        public override Part PartType { get => Part.leg; }
    }
}
