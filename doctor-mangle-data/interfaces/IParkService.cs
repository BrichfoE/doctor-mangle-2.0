using doctor_mangle_data.models;
using System;

namespace doctor_mangle.interfaces
{
    public interface IParkService
    {
        ParkData[] GenerateParks();

        ParkData[] AddParts(ParkData[] locations, Random RNG, int playerCount);

        ParkData[] HalveParts(ParkData[] locations);
    }
}
