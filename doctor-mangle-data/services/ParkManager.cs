using doctor_mangle.models.factories;
using DrMangle.Model;
using Newtonsoft.Json;
using System;

namespace DrMangle
{
    public class ParkManager
    {
        public ParkData[] Locations { get; set; }

        [JsonConstructor]
        public ParkManager()
        {
            Locations = new ParkData[6];         
        }

        public ParkManager(Random RNG, int players)
        {
            Locations = new ParkData[6];
            GeneratePark(Locations);
            Locations[0] = new ParkData("Lab", 0);
            Locations[5] = new ParkData("Arena", 0);
            AddParts(RNG, players);
        }

        private void GeneratePark(ParkData[] parks)
        {
            for (int i = 1; i < 5; i++)
            {
                string name = "";
                switch (i)
                {
                    case 1:
                        name = "Backyard";
                        break;
                    case 2:
                        name = "Boneyard";
                        break;
                    case 3:
                        name = "Junkyard";
                        break;
                    case 4:
                        name = "Rockyard";
                        break;
                    default:
                        throw new System.ArgumentException("No name for Park[" + i + "]", "original"); ;
                }
                Locations[i] = new ParkData(name, i);
            }
            
        }

        public void AddParts(Random RNG, int players)
        {
            for (int i = 1; i < 5; i++)
            {
                int roll = RNG.Next(1, players * 5);

                for (int j = 0; j < roll; j++)
                {
                    var partRoll = RNG.Next(1, 4);
                    BodyPartFactory factory;
                    switch (partRoll)
                    {
                        case 1:
                            factory = new HeadFactory(Locations[i].ParkPart);
                            break;
                        case 2:
                            factory = new TorsoFactory(Locations[i].ParkPart);
                            break;
                        case 3:
                            factory = new ArmFactory(Locations[i].ParkPart);
                            break;
                        case 4:
                            factory = new LegFactory(Locations[i].ParkPart);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("ParkManager.AddParts - random role outside 1-4");
                            break;
                    }
                    var bodyPart = factory.BodyPart;
                    Locations[i].PartsList.AddLast(bodyPart);
                }
            }
            
        }

        public void HalveParts()
        {
            for (int i = 1; i < 5; i++)
            {
                decimal count = Locations[i].PartsList.Count;
                for (int j = 0; j < Math.Round((count/2), 0); j++)
                {
                    Locations[i].PartsList.RemoveFirst();
                }
            }
        }
        
    }
}
