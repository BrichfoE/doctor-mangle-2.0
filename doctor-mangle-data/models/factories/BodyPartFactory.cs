using doctor_mangle.constants;
using doctor_mangle.models.parts;

namespace doctor_mangle.models.factories
{
    // Attaboy: Factory pattern, because different types of parts will have different stat making logic methods
    public abstract class BodyPartFactory
    {
        protected BodyPart _bodyPart;
        public BodyPart BodyPart { get => _bodyPart; }
        public BodyPartFactory(Structure? structure)
        {
            this.GenerateBodyPart(structure);
        }
        public abstract void GenerateBodyPart(Structure? structure);

        // todo: previously had capped all stats at 16666.5f.  Why?
    }
}
