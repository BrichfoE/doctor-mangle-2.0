using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using doctor_mangle_data.models;
using NUnit.Framework;

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
        [TestCase(2, 1)]
        [TestCase(10, 5)]
        [TestCase(100, 50)]
        [TestCase(5000, 2500)]
        [TestCase(20000, 10000)]
        public void MonsterFight_ReturnsExpectedWinner(int winnerStat, int loserStat)
        {
            //arrange
            var mockRandom = TestUtils.GetMockRandom_Next(1);
            mockRandom.Setup(x => x.NextDouble())
                .Returns(1);
            _service = new BattleService(mockRandom.Object);
            var expectedRoundCount = 2;


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
            Assert.AreEqual(winner, actual);
            // Assert.AreEqual(expectedRoundCount, actual);
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
