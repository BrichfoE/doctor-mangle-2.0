using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using doctor_mangle_data.models;
using Microsoft.Extensions.Logging;
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
            var mLogger = new Mock<ILogger<IGameService>>();
            var mPlayerService = new Mock<IPlayerService>();
            var mParkService = new Mock<IParkService>();
            var mRandom = new Mock<Random>();
            var mPartComparer = new Mock<IComparer<BodyPart>>();
            return new GameService(
                mLogger.Object,
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
            part.PartDurability = 1;
            return part;
        }

        private void AddParts(List<BodyPart> parts, Rarity head, Rarity torso, Rarity arm)
        {
            parts.Add(GetPart(Part.head, head));
            parts.Add(GetPart(Part.torso, torso));
            parts.Add(GetPart(Part.arm, arm));
        }

        private ParkData GetPark(int partLen)
        {
            var park = new ParkData();
            for (int i = 0; i < partLen; i++)
            {
                park.PartsList.AddLast(new Head());
            }
            return park;
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
            var mLogger = new Mock<ILogger<IGameService>>();
            var gameService = new GameService(
                mLogger.Object,
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

        [Test]
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(0, 3)]
        [TestCase(2, 1)]
        [TestCase(5, 3)]
        [TestCase(1, 0)]
        [TestCase(5, 0)]
        public void AISearchTurn_ReturnAssignsPartIfNotNull(int players, int parts)
        {
            // Arrange
            var parkInt = 1;

            var mLogger = new Mock<ILogger<IGameService>>();
            var mPlayerService = new Mock<IPlayerService>();
            var mParkService = new Mock<IParkService>();
            var mRandom = TestUtils.GetMockRandom_Next(parkInt);
            var mPartComparer = new Mock<IComparer<BodyPart>>();
            var gameService = new GameService(
                mLogger.Object,
                mPlayerService.Object,
                mParkService.Object,
                mPartComparer.Object,
                mRandom.Object);

            var playerList = new List<PlayerData>();
            for (int i = 0; i < players; i++)
            {
                playerList.Add(new PlayerData() { Name = $"{i}: {parts - i-1 >= 0}" });
            };
            var parks = new ParkData[6];
            parks[parkInt] = GetPark(parts);
            var gd = new GameData()
            {
                Parks = parks,
                AiPlayers = playerList.ToArray()
            };

            var expectedSum = Math.Max(parts - players, 0);

            // Act
            gameService.AISearchTurn(gd, 1);

            // Assert
            Assert.AreEqual(expectedSum, gd.Parks[parkInt].PartsList.Count);
            foreach (var player in gd.AiPlayers)
            {
                if (player.Name.Contains("True"))
                {
                    Assert.IsNotNull(player.Bag[0]);
                }
                else
                {
                    Assert.IsNull(player.Bag[0]);
                }
            }
        }

        // TODO:  this needs to be broken out into different cases
        [Test]
        public void AIBuildTurn()
        {
            var gameService = GetService();
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

            var players = new PlayerData[]
            {
                new PlayerData(){ WorkshopCuppoard = new List<BodyPart>(), Name = "SansParts" },
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = best, Name = "BestMonster"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = limb, Name = "BetterLimbs"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Name = "SansMonster" },
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = head, Name = "BetterHead"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = tors, Name = "BetterTorso"},
                new PlayerData(){ WorkshopCuppoard = workshopBase.ToList(), Monster = both, Name = "BetterBoth"}
            };
            var gameData = new GameData()
            {
                AiPlayers = players
            };

            // Act
            gameService.AIBuildTurn(gameData);

            // Assert
            Assert.IsNull(gameData.AiPlayers[0].Monster);
            Assert.AreEqual(0, gameData.AiPlayers[0].WorkshopCuppoard.Count);
            for (int i = 1; i < players.Length; i++)
            {
                Assert.IsNotNull(gameData.AiPlayers[i].Monster);
                Assert.AreEqual(0, gameData.AiPlayers[i].WorkshopCuppoard.Count);
            }
        }


    }
}
