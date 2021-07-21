using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models.factories;
using doctor_mangle.models.parts;
using doctor_mangle_data.models;
using System;
using System.Linq;

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

        public void MovePartsForSerilaization<T>(T objectWithCollection)
        {
            if (objectWithCollection.GetType() != typeof(ParkData))
            {
                throw new ArgumentException("Must use player in PlayerService serialization implementation");
            }

            var park = objectWithCollection as ParkData;

            park._heads = park.PartsList
                .Where(x => x.GetType() == typeof(Head))
                .Select(y => new Head(y))
                .ToList();

            park._torsos = park.PartsList
                .Where(x => x.GetType() == typeof(Torso))
                .Select(y => new Torso(y))
                .ToList();

            park._arms = park.PartsList
                .Where(x => x.GetType() == typeof(Arm))
                .Select(y => new Arm(y))
                .ToList();

            park._legs = park.PartsList
                .Where(x => x.GetType() == typeof(Leg))
                .Select(y => new Leg(y))
                .ToList();

            park.PartsList.Clear();
        }
        public void MovePartsAfterDeserilaization<T>(T objectWithCollection)
        {

            if (objectWithCollection.GetType() != typeof(ParkData))
            {
                throw new ArgumentException("Must use player in PlayerService serialization implementation");
            }

            var park = objectWithCollection as ParkData;

            var sum = park._arms.Count
                + park._torsos.Count
                + park._heads.Count
                + park._legs.Count;
            while (sum > 0)
            {
                var prt = _rng.Next(1, 4);
                if (prt == 1 && park._arms.Count > 0)
                {
                    park.PartsList.AddLast(park._arms[0]);
                    park._arms.RemoveAt(0);
                    sum--;
                }
                else if (prt == 2 && park._torsos.Count > 0)
                {
                    park.PartsList.AddLast(park._torsos[0]);
                    park._torsos.RemoveAt(0);
                    sum--;
                }
                else if (prt == 3 && park._heads.Count > 0)
                {
                    park.PartsList.AddLast(park._heads[0]);
                    park._heads.RemoveAt(0);
                    sum--;
                }
                else if (prt == 4 && park._legs.Count > 0)
                {
                    park.PartsList.AddLast(park._legs[0]);
                    park._legs.RemoveAt(0);
                    sum--;
                }
            }
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
