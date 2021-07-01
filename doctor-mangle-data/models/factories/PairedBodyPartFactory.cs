using doctor_mangle.constants;
using doctor_mangle.models.parts;

namespace doctor_mangle.models.factories
{
    // Attaboy: Factory pattern, because different types of parts will have different stat making logic methods
    public abstract class PairedBodyPartFactory : BodyPartFactory
    {
        protected new PairedBodyPart _bodyPart;
        public new PairedBodyPart BodyPart { get => _bodyPart; }
        public PairedBodyPartFactory(Structure? structure) : base(structure)
        {
        }
    }
}
