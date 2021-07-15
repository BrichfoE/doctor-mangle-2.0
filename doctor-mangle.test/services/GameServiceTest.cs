using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle_data.models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doctor_mangle.test.services
{
    [TestFixture]
    public class GameServiceTest
    {
        private IGameService GetService()
        {
            var mPlayerService = new Mock<IPlayerService>();
            var mParkService = new Mock<IParkService>();
            var mRandom = new Mock<Random>();
            var mPartComparer = new Mock<IComparer<BodyPart>>();
            return new GameService(
                mPlayerService.Object,
                mParkService.Object,
                mPartComparer.Object,
                mRandom.Object);
        }

        private BodyPart GetPart(Part type, Rarity rarity)
        {
            BodyPart part;
            switch (type)
            {
                case Part.head:
                    part = new Head();
                    break;
                case Part.torso:
                    part = new Torso();
                    break;
                case Part.arm:
                    part = new Arm();
                    break;
                case Part.leg:
                    part = new Leg();
                    break;
                default:
                    part = new Head();
                    break;
            }
            part.PartRarity = rarity;
            return part;
        }

        private void AddParts(List<BodyPart> parts, Rarity head, Rarity torso, Rarity arm)
        {
            parts.Add(GetPart(Part.head, head));
            parts.Add(GetPart(Part.torso, torso));
            parts.Add(GetPart(Part.arm, arm));
        }

        [Test]
        [TestCase("test_1", 1, 1)]
        [TestCase("test_2", 4, 17)]
        [TestCase("test_3", 0, 42)]
        [TestCase("test_4", 99, 108)]
        public void GetNewGameData(string name, int aiCount, int gameID)
        {
            // arrange
            var expectedParks = new ParkData[6] {
                new ParkData() { ParkName = "Lab" },
                new ParkData() { ParkName = "Backyard" },
                new ParkData() { ParkName = "Boneyard" },
                new ParkData() { ParkName = "Junkyard" },
                new ParkData() { ParkName = "Rockyard" },
                new ParkData() { ParkName = "Arena" }
            };
            var mPlayerService = new Mock<IPlayerService>();
            mPlayerService.Setup(x => x.GeneratePlayer(
                    It.IsAny<string>(),
                    It.Is<bool>(x => x == false)))
                .Returns(new PlayerData() { Name = name });
            mPlayerService.Setup(x => x.GeneratePlayer(
                    It.IsAny<string>(),
                    It.Is<bool>(x => x == true)))
                .Returns(new PlayerData());

            var mParkService = new Mock<IParkService>();
            mParkService.Setup(x => x.GenerateParks())
                .Returns(new ParkData[6]);
            mParkService.Setup(x => x.AddParts(
                    It.IsAny<ParkData[]>(),
                    It.IsAny<int>()))
                .Returns(expectedParks);

            var expected = new GameData()
            {
                Parks = expectedParks,
                CurrentPlayer = new PlayerData(),
                AiPlayers = new PlayerData[aiCount]
            };
            for (int i = 0; i < aiCount; i++)
            {
                expected.AiPlayers[i] = new PlayerData(); 
            }

            var mRandom = new Mock<Random>();
            var mPartComparer = new Mock<IComparer<BodyPart>>();
            var gameService = new GameService(
                mPlayerService.Object,
                mParkService.Object,
                mPartComparer.Object,
                mRandom.Object);

            // act
            var actual = gameService.GetNewGameData(name, aiCount, gameID);

            // assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(gameID, actual.GameDataId);
            Assert.AreEqual(name, actual.GameName);
            Assert.IsInstanceOf(typeof(List<MonsterGhost>), actual.Graveyard);
            Assert.IsInstanceOf(typeof(PlayerData), actual.CurrentPlayer);
            Assert.AreEqual(6, actual.Parks.Length);
            Assert.AreEqual(expectedParks, actual.Parks);
            Assert.AreEqual(name, actual.CurrentPlayer.Name);
            Assert.AreEqual(aiCount, actual.AiPlayers.Length);
            foreach (var ai in actual.AiPlayers)
            {
                Assert.AreNotEqual(name, ai.Name);
            }
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void PrintRegionOptions_ZeroThroughFive_ReturnsCorrectString(int currentRegion)
        {
            // arrange
            var gameService = GetService();
            var regions = new ParkData[6] {
                new ParkData() { ParkName = "Lab" },
                new ParkData() { ParkName = "Backyard" },
                new ParkData() { ParkName = "Boneyard" },
                new ParkData() { ParkName = "Junkyard" },
                new ParkData() { ParkName = "Rockyard" },
                new ParkData() { ParkName = "Arena" }
            };

            var gameData = new GameData() { CurrentRegion = currentRegion, Parks = regions };
            var expected = string.Empty;
            for (int i = 1; i < 5; i++)
            {
                if (currentRegion == i)
                {
                    expected += $"{i} - Stay in the {regions[i].ParkName} \r\n";
                }
                else
                {
                    expected += $"{i} - Go to the {regions[i].ParkName} \r\n";
                }
            }

            // act
            var actual = gameService.PrintRegionOptions(gameData);

            // assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        // [Test]
        public void AISearchTurn(GameData gd, int round)
        {

        }

        [Test]
        public void AIBuildTurn()
        {
            // Arrange
            var mPlayerService = new Mock<IPlayerService>();
            var mParkService = new Mock<IParkService>();
            var mRandom = new Mock<Random>();
            var mPartComparer = new Mock<IComparer<BodyPart>>();
            var gameService = new GameService(
                mPlayerService.Object,
                mParkService.Object,
                mPartComparer.Object,
                mRandom.Object);

            var workshopBase = new List<BodyPart>();
            this.AddParts(workshopBase, Rarity.Legendary, Rarity.Legendary, Rarity.Legendary);

            var best = new MonsterData() { Name = "best" };
            AddParts(best.Parts, Rarity.Unicorn, Rarity.Unicorn, Rarity.Unicorn);
            var limb = new MonsterData() { Name = "limb" };
            AddParts(limb.Parts, Rarity.Unicorn, Rarity.Unicorn, Rarity.Common);
            var head = new MonsterData() { Name = "head" };
            AddParts(head.Parts, Rarity.Common, Rarity.Unicorn, Rarity.Unicorn);
            var tors = new MonsterData() { Name = "tors" };
            AddParts(tors.Parts, Rarity.Unicorn, Rarity.Common, Rarity.Unicorn);
            var both = new MonsterData() { Name = "both" };
            AddParts(both.Parts, Rarity.Common, Rarity.Common, Rarity.Common);
            var oldLimb = limb.Parts[2];

            var players = new PlayerData[]
            {
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = best, Name = "BestMonster"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = limb, Name = "BetterLimbs"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Name = "SansMonster" },
                new PlayerData(){ WorkshopCuppoard = new List<BodyPart>(), Name = "SansParts" },
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = head, Name = "BetterHead"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = tors, Name = "BetterTorso"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = both, Name = "BetterBoth"}
            };
            var gameData = new GameData() 
            { 
                Graveyard = new List<MonsterGhost>(),
                AiPlayers = players
            };

            // Act
            gameService.AIBuildTurn(gameData);

            // Assert
            // we killed monsters that had better heads and torsos
            // Assert.AreEqual(3, gameData.Graveyard.Count);
            Assert.IsTrue(gameData.Graveyard.Any(x => x.Name == "head"));
            Assert.IsTrue(gameData.Graveyard.Any(x => x.Name == "tors"));
            // Assert.IsTrue(gameData.Graveyard.Any(x => x.Name == "both"));
            // Assert.AreEqual(best, gameData.AiPlayers[0].Monster);
            // Assert.AreEqual(limb, gameData.AiPlayers[1].Monster);
            // Assert.AreNotEqual(oldLimb, gameData.AiPlayers[1].Monster.Parts[2]);
            // Assert.IsNotNull(gameData.AiPlayers[2].Monster);
            Assert.IsNull(gameData.AiPlayers[3].Monster);
        }


    }
}
