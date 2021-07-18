using doctor_mangle.constants;

namespace doctor_mangle.models.parts
{
    public class Arm : PairedBodyPart
    {
        public override Part PartType { get => Part.arm; }
    }
}
