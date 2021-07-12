using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using NUnit.Framework;
using System.Collections.Generic;

namespace doctor_mangle.test.services
{
    [TestFixture]
    public class BattleServiceTest
    {
        // public BattleServiceTest(IBattleService battleService)
        // {
        //     _service = battleService;
        // }

        private IBattleService _service;

        private MonsterData GenerateMonster(float stat, decimal durability, string? name)
        {
            var monster = new MonsterData() { Name = name };
            monster.Parts.Add(new Head());
            monster.Parts.Add(new Torso());
            monster.Parts.Add(new Arm() { IsLeftSide = true });

            foreach (var part in monster.Parts)
            {
                part.PartStats[Stat.Alacrity] = stat;
                part.PartStats[Stat.Strength] = stat;
                part.PartStats[Stat.Endurance] = stat;
                part.PartStats[Stat.Technique] = stat;
                part.PartRarity = Rarity.Common;
                part.PartStructure = Structure.Animal;
                part.PartDurability = durability;
            }

            return monster;
        }

        [Test]
        [TestCase(2, 1, 3)]
        [TestCase(10, 5, 2)]
        [TestCase(100, 50, 2)]
        [TestCase(5000, 2500, 2)]
        [TestCase(20000, 10000, 2)]
        [TestCase(1, 1, 5)]
        [TestCase(5, 5, 4)]
        [TestCase(50, 50, 4)]
        [TestCase(1000, 1000, 4)]
        [TestCase(10000, 10000, 4)]
        [TestCase(10, 1, 1)]
        public void MonsterFight_ReturnsExpectedWinnerAndRoundCount(int winnerStat, int loserStat, int expectedCount)
        {
            //arrange
            var mockRandom = TestUtils.GetMockRandom_Next(1);
            mockRandom.Setup(x => x.NextDouble())
                .Returns(1);
            _service = new BattleService(mockRandom.Object);


            var winner = new PlayerData()
            {
                Name = "winner",
                Monster = this.GenerateMonster(winnerStat, 1, "winning_monster")
            };
            var loser = new PlayerData()
            {
                Name = "loser",
                Monster = this.GenerateMonster(loserStat, 1, "losing_monster"),
            };

            //act
            var actual = _service.MonsterFight(winner, loser);

            //assert
            Assert.AreEqual(winner, actual.Winner);
            Assert.AreEqual(expectedCount, actual.Rounds.Count);
        }

        [Test]
        public void MonsterFight_ReturnsExpectedScriptText_OneRound()
        {
            //arrange
            var mockRandom = TestUtils.GetMockRandom_Next(1);
            mockRandom.Setup(x => x.NextDouble())
                .Returns(1);
            _service = new BattleService(mockRandom.Object);
            var expectedScript = new List<string>()
            {
                "Draw your eyes to the arena!",
                "In the blue corner, winner presents winning_monster",
                "winning_monster boasts 0 wins!",
                "In the green corner loser presents losing_monster",
                "losing_monster boasts 0 wins!",
                "winning_monster swings at losing_monster's Common Animal torso!",
                "Common Animal torso goes from 1 to -1.57142871156032",
                "doctor_mangle.models.monsters.MonsterData's Common Animal torso has been destroyed!",
                "doctor_mangle.models.PlayerData's opponent can no longer put a fight.\r\ndoctor_mangle.models.PlayerData is victorious!"
            };

            var winner = new PlayerData()
            {
                Name = "winner",
                Monster = this.GenerateMonster(10, 1, "winning_monster")
            };
            var loser = new PlayerData()
            {
                Name = "loser",
                Monster = this.GenerateMonster(1, 1, "losing_monster"),
            };

            //act
            var actual = _service.MonsterFight(winner, loser);

            //assert
            Assert.AreEqual(expectedScript.Count, actual.Text.Count);
            for (int i = 0; i < actual.Text.Count; i++)
            {
                Assert.AreEqual(expectedScript[i], actual.Text[i]);
            }
        }

        [Test]
        public void MonsterFight_ReturnsExpectedScriptText_TwoRounds()
        {
            //arrange
            var mockRandom = TestUtils.GetMockRandom_Next(1);
            mockRandom.Setup(x => x.NextDouble())
                .Returns(1);
            _service = new BattleService(mockRandom.Object);
            var expectedScript = new List<string>()
            {
                "Draw your eyes to the arena!",
                "In the blue corner, winner presents winning_monster",
                "winning_monster boasts 0 wins!",
                "In the green corner loser presents losing_monster",
                "losing_monster boasts 0 wins!",
                "winning_monster swings at losing_monster's Common Animal torso!",
                "Common Animal torso goes from 1 to 0.228571386531905",
                "losing_monster counters at winning_monster's Common Animal torso!",
                "Common Animal torso goes from 1 to 0.9052631616988669",
                "winning_monster swings at losing_monster's Common Animal torso!",
                "Common Animal torso goes from 0.228571386531905 to -0.483294131855020",
                "doctor_mangle.models.monsters.MonsterData's Common Animal torso has been destroyed!",
                "doctor_mangle.models.PlayerData's opponent can no longer put a fight.\r\ndoctor_mangle.models.PlayerData is victorious!"
            };

            var winner = new PlayerData()
            {
                Name = "winner",
                Monster = this.GenerateMonster(3, 1, "winning_monster")
            };
            var loser = new PlayerData()
            {
                Name = "loser",
                Monster = this.GenerateMonster(1, 1, "losing_monster"),
            };

            //act
            var actual = _service.MonsterFight(winner, loser);

            //assert
            Assert.AreEqual(expectedScript.Count, actual.Text.Count);
            for (int i = 0; i < actual.Text.Count; i++)
            {
                Assert.AreEqual(expectedScript[i], actual.Text[i]);
            }
        }


        [Test]
        [TestCase(true, 0, Part.head)]
        [TestCase(true, 1, Part.torso)]
        [TestCase(false, 0, Part.head)]
        [TestCase(false, 1, Part.torso)]
        [TestCase(false, 2, Part.arm)]
        public void GetTarget_Criticalhit(bool crit, int roll, Part expected)
        {
            //arrange
            var mockRandom = TestUtils.GetMockRandom_Next(roll);
            _service = new BattleService(mockRandom.Object);

            var winner = GenerateMonster(1, 1, "target_monster");

            //act
            var actual = _service.GetTarget(winner, "attacker", crit);

            //assert
            Assert.AreEqual(expected, actual.PartType);
        }

        [Test]
        public void GrantCash_ReturnsExpectedWinner()
        {
            //arrange
            var mockRandom = TestUtils.GetMockRandom_NextDouble(1);
            _service = new BattleService(mockRandom.Object);

            //act
            _service.GrantCash(new PlayerData(), 0);

            //assert
            Assert.IsTrue(true);
        }
    }
}
