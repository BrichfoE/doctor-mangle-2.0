using doctor_mangle.models.parts;

namespace doctor_mangle.interfaces
{
    public interface IPartService
    {
        string GetPartDetails(BodyPart part);
    }
}
