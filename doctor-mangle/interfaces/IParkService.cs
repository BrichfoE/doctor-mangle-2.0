using doctor_mangle_data.models;

namespace doctor_mangle.interfaces
{
    public interface IParkService : IPartsCollectionSerializer
    {
        ParkData[] GenerateParks();

        ParkData[] AddParts(ParkData[] locations, int playerCount);

        ParkData[] HalveParts(ParkData[] locations);
    }
}
