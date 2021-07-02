using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using doctor_mangle_data.models;
using Moq;
using NUnit.Framework;
using System;

namespace doctor_mangle.test.services
{
    [TestFixture]
    public class ParkServiceTest
    {
        // public ParkServiceTest(IParkServiceT parkServiceTest)
        // {
        //     _service = parkServiceTest;
        // }
        
        private IParkService _service;

        [OneTimeSetUp]
        public void Init()
        {
            _service = new ParkService();
        }

        [Test]
        public void GenerateParks_ReturnsNotNull_ExpectedArrayLength()
        {
            //arrange
            var expectedLength = 6;

            //act
            var actual = _service.GenerateParks();

            //assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedLength, actual.Length);
        }

        [Test]
        public void GenerateParks_ReturnsExpectedData()
        {
            //arrange
            var expectedNames = new string[6]
            {
                "Lab",
                "Backyard",
                "Boneyard",
                "Junkyard",
                "Rockyard",
                "Arena"
            };
            var expectedStructures = new Structure[6]
            {
                0,
                Structure.Animal,
                Structure.Human,
                Structure.Mechanical,
                Structure.Rock,
                0
            };
            
            //act
            var actual = _service.GenerateParks();

            //assert
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(expectedNames[i], actual[i].ParkName);
                Assert.AreEqual(expectedStructures[i], actual[i].ParkPart);
            }
        }

        [Test]
        [TestCase(Structure.Animal, Part.head)]
        [TestCase(Structure.Human, Part.head)]
        [TestCase(Structure.Mechanical, Part.head)]
        [TestCase(Structure.Rock, Part.head)]
        [TestCase(Structure.Animal, Part.torso)]
        [TestCase(Structure.Animal, Part.leg)]
        [TestCase(Structure.Animal, Part.arm)]
        public void AddParts(Structure structure, Part partType)
        {
            // arrange
            var parks = new ParkData[1] { 
                new ParkData() { 
                    ParkPart = structure
                } 
            };
            var mockRng = new Mock<Random>();
            mockRng.Setup(x => x.Next(
                It.IsAny<int>(), 
                It.IsAny<int>()))
                .Returns((int)partType);

            // act
            var actual = _service.AddParts(parks, mockRng.Object, 1);

            // assert
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual((int)partType, actual[0].PartsList.Count);
            foreach (var part in actual[0].PartsList)
            {
                Assert.AreEqual(structure, part.PartStructure);
                Assert.AreEqual(partType, part.PartType);
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(15)]
        [TestCase(100)]
        [TestCase(10000)]
        public void HalveParts_ReturnsNotNull(int initialLength)
        {
            //arrange
            var expectedLength = initialLength / 2;
            var parkData = new ParkData();
            for (int i = 0; i < initialLength; i++)
            {
                parkData.PartsList.AddLast(new Head());
            }
            var locations = new ParkData[1] { parkData };
            
            //act
            var actual = _service.HalveParts(locations);

            //assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expectedLength, actual[0].PartsList.Count);
        }
    }
}


//public ParkData[] AddParts(ParkData[] locations, Random RNG, int playerCount)
//{
//    for (int i = 1; i < locations.Length; i++)
//    {
//        int roll = RNG.Next(1, playerCount * 5);

//        for (int j = 0; j < roll; j++)
//        {
//            var partRoll = RNG.Next(1, 4);
//            BodyPartFactory factory;
//            switch (partRoll)
//            {
//                case 1:
//                    factory = new HeadFactory(locations[i].ParkPart);
//                    break;
//                case 2:
//                    factory = new TorsoFactory(locations[i].ParkPart);
//                    break;
//                case 3:
//                    factory = new ArmFactory(locations[i].ParkPart);
//                    break;
//                case 4:
//                    factory = new LegFactory(locations[i].ParkPart);
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException("ParkManager.AddParts - random role outside 1-4");
//                    break;
//            }
//            var bodyPart = factory.BodyPart;
//            locations[i].PartsList.AddLast(bodyPart);

//        }
//    }
//    return locations;
//}
