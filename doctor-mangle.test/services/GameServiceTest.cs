using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle_data.models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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

        // [Test]
        public void AIBuildTurn(GameData data)
        {

        }


    }
}
