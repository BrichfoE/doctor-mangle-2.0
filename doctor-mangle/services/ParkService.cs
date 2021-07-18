using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models.factories;
using doctor_mangle_data.models;
using System;

namespace doctor_mangle.services
{
    public class ParkService : IParkService
    {
        private readonly Random _rng;
        public ParkService(Random random)
        {
            this._rng = random;
        }
        public ParkData[] GenerateParks()
        {
            var _locations = new ParkData[6];
            _locations[0] = new ParkData() { ParkName = "Lab" };
            _locations[1] = new ParkData() { ParkName = "Backyard", ParkPart = Structure.Animal };
            _locations[2] = new ParkData() { ParkName = "Boneyard", ParkPart = Structure.Human };
            _locations[3] = new ParkData() { ParkName = "Junkyard", ParkPart = Structure.Mechanical };
            _locations[4] = new ParkData() { ParkName = "Rockyard", ParkPart = Structure.Rock };
            _locations[5] = new ParkData() { ParkName = "Arena" };
            return _locations;
        }

        public ParkData[] AddParts(ParkData[] locations, int playerCount)
        {
            for (int i = 0; i < locations.Length; i++)
            {
                if (locations[i].ParkPart != 0)
                {
                    int roll = _rng.Next(1, playerCount * 5);

                    for (int j = 0; j < roll; j++)
                    {
                        var partRoll = _rng.Next(1, 4);
                        BodyPartFactory factory;
                        switch (partRoll)
                        {
                            case 1:
                                factory = new HeadFactory(locations[i].ParkPart);
                                break;
                            case 2:
                                factory = new TorsoFactory(locations[i].ParkPart);
                                break;
                            case 3:
                                factory = new ArmFactory(locations[i].ParkPart);
                                break;
                            case 4:
                                factory = new LegFactory(locations[i].ParkPart);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("ParkManager.AddParts - random role outside 1-4");
                        }
                        var bodyPart = factory.BodyPart;
                        locations[i].PartsList.AddLast(bodyPart);
                    }
                }
            }
            return locations;
        }

        public ParkData[] HalveParts(ParkData[] locations)
        {
            for (int i = 0; i < locations.Length; i++)
            {
                decimal count = Math.Ceiling(locations[i].PartsList.Count / (decimal)2);
                for (int j = 0; j < count; j++)
                {
                    locations[i].PartsList.RemoveFirst();
                }
            }
            return locations;
        }

    }
}
