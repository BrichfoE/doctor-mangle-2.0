using doctor_mangle_data.models;

namespace doctor_mangle.interfaces
{
    public interface IParkService
    {
        ParkData[] GenerateParks();

        void MovePartsForSerilaization(ParkData park);

        void MovePartsAfterDeserilaization(ParkData park);

        ParkData[] AddParts(ParkData[] locations, int playerCount);

        ParkData[] HalveParts(ParkData[] locations);
    }
}
