using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace doctor_mangle.test.services
{
    [TestFixture]
    public class PlayerServiceTest
    {
        private IPlayerService GenerateService(double expected)
        {
            var mRandom = TestUtils.GetMockRandom_NextDouble(expected);
            var mCompare = new Mock<IComparer<BodyPart>>();
            return new PlayerService(mCompare.Object, mRandom.Object);
        }

        [Test]
        [TestCase("", true)]
        [TestCase("", false)]
        [TestCase("GivenName", true)]
        [TestCase("GivenName", false)]
        [TestCase(null, true)]
        [TestCase(null, false)]
        public void GeneratePlayer_ReturnsNotNull(string name, bool isAI)
        {
            // arrange
            var playerService = GenerateService(0);

            var expectedName = isAI || name == null || name == ""
                ? "Cool Luke"
                : name;

            // act
            PlayerData result = playerService.GeneratePlayer(name, isAI);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedName, result.Name);
            Assert.AreEqual(isAI, result.IsAI);
            Assert.AreEqual(0, result.WinsCount);
            Assert.AreEqual(0, result.FightsCount);
            Assert.AreEqual(0, result.Luck);
            Assert.IsNull(result.Monster);
            Assert.IsNotNull(result.Bag);
            Assert.AreEqual(5, result.Bag.Length);
            Assert.IsNotNull(result.WorkshopCuppoard);
            Assert.AreEqual(0, result.WorkshopCuppoard.Count);
            Assert.IsNotNull(result.SpareParts);
            Assert.AreEqual(5, result.SpareParts.Count);
        }

        [Test]
        [TestCase(1)]
        [TestCase(0)]
        [TestCase(.5)]
        [TestCase(.25)]
        [TestCase(.71865)]
        [TestCase(.79865)]
        public void GenerateRandomName_ReturnsExpected(double roll)
        {
            // arrange
            var playerService = GenerateService(roll);

            double adjIndex = roll * (StaticReference.adjectives.Length - 1);
            double nmeIndex = roll * (StaticReference.names.Length - 1);

            var adj = StaticReference.adjectives[(int)adjIndex];
            var nme = StaticReference.names[(int)nmeIndex];
            var expected = adj + " " + nme;

            // act
            string result = playerService.GenerateRandomName();

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CheckBag_ReturnsExpected(bool isAI)
        {
            // arrange
            var playerService = GenerateService(0);

            var testPlayer = new PlayerData()
            {
                IsAI = isAI
            };
            testPlayer.Bag[0] = new Head() { PartRarity = Rarity.Common, PartStructure = Structure.Animal };
            testPlayer.Bag[1] = new Torso() { PartRarity = Rarity.Common, PartStructure = Structure.Animal };
            testPlayer.Bag[3] = new Leg() { PartRarity = Rarity.Common, PartStructure = Structure.Animal, IsLeftSide = false };
            testPlayer.Bag[4] = new Leg() { PartRarity = Rarity.Common, PartStructure = Structure.Animal, IsLeftSide = true };

            string expected = isAI
                ? "Hands off!"
                : "1 - Common Animal head\r\n" +
                    "2 - Common Animal torso\r\n" +
                    "3 - Common Animal right leg\r\n" +
                    "4 - Common Animal left leg\r\n";

            // act
            string result = playerService.CheckBag(testPlayer);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
            Console.WriteLine(result);
        }

        [Test]
        [TestCase(Rarity.Common, 1, 1)]
        [TestCase(Rarity.Common, 1, 0)]
        [TestCase(Rarity.Common, 1, .5)]
        [TestCase(Rarity.Common, 1, .25)]
        [TestCase(Rarity.Common, 0, 1)]
        [TestCase(Rarity.Common, 0, 0)]
        [TestCase(Rarity.Common, 0, .5)]
        [TestCase(Rarity.Common, 0, .25)]
        [TestCase(Rarity.Unicorn, 1, 1)]
        [TestCase(Rarity.Unicorn, 1, 0)]
        [TestCase(Rarity.Unicorn, 1, .5)]
        [TestCase(Rarity.Unicorn, 1, .25)]
        [TestCase(Rarity.Unicorn, 0, 1)]
        [TestCase(Rarity.Unicorn, 0, 0)]
        [TestCase(Rarity.Unicorn, 0, .5)]
        [TestCase(Rarity.Unicorn, 0, .25)]
        [TestCase(Rarity.Rare, 1, 1)]
        [TestCase(Rarity.Rare, 1, 0)]
        [TestCase(Rarity.Rare, 1, .5)]
        [TestCase(Rarity.Rare, 0, 1)]
        [TestCase(Rarity.Rare, 0, 0)]
        [TestCase(Rarity.Rare, 0, .5)]
        [TestCase(Rarity.Mythic, 1, 1)]
        [TestCase(Rarity.Mythic, 1, 0)]
        [TestCase(Rarity.Mythic, 1, .5)]
        [TestCase(Rarity.Mythic, 0, 1)]
        [TestCase(Rarity.Mythic, 0, 0)]
        [TestCase(Rarity.Mythic, 0, .5)]
        [TestCase(Rarity.Legendary, 0, 1)]
        [TestCase(Rarity.Legendary, 0, 0)]
        [TestCase(Rarity.Legendary, 0, .5)]
        [TestCase(Rarity.Epic, 0, 1)]
        [TestCase(Rarity.Epic, 0, 0)]
        [TestCase(Rarity.Epic, 0, .5)]
        public void ScrapItem_ReturnsExpected(Rarity rarity, double roll, decimal durability)
        {
            // arrange
            var playerService = GenerateService(roll);

            var partType = Structure.Animal;
            var scraps = new Dictionary<Structure, int>() { { partType, 10 } };
            var parts = new List<BodyPart>() {
                new Torso(),
                new Head()
                {
                    PartStructure = partType,
                    PartDurability = durability,
                    PartRarity = rarity
                },
                new Torso()
            };

            double max = StaticReference.PartsPerRarity[rarity];

            int expectedAmount = (int)(roll * max * (double)durability);
            string expected = $"You salvaged {expectedAmount} {partType} parts from a {rarity} {partType} head.";

            // act
            string result = playerService.ScrapItem(scraps, parts, 1);

            // assert
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual(expectedAmount + 10, scraps[partType]);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(Rarity.Common, 1)]
        [TestCase(Rarity.Unicorn, 1)]
        [TestCase(Rarity.Epic, 1)]
        [TestCase(Rarity.Common, 0)]
        [TestCase(Rarity.Unicorn, 0)]
        [TestCase(Rarity.Epic, 0)]
        [TestCase(Rarity.Legendary, 0)]
        [TestCase(Rarity.Common, 0.25)]
        [TestCase(Rarity.Unicorn, 0.5)]
        [TestCase(Rarity.Epic, 0.3333)]
        [TestCase(Rarity.Legendary, 0.795)]
        public void GetRepairCost_ReturnsNotNull(Rarity rarity, decimal durability)
        {
            // arrange
            var playerService = GenerateService(0);
            var testPart = new Head()
            {
                PartDurability = durability,
                PartRarity = rarity
            };

            double fullPrice = StaticReference.PartsPerRarity[rarity];
            double toFix = (double)(1 - durability);

            int expected = (int)(fullPrice * toFix);

            // act
            int result = playerService.GetRepairCost(testPart);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(Rarity.Common, 1)]
        [TestCase(Rarity.Common, .5)]
        [TestCase(Rarity.Common, 0)]
        [TestCase(Rarity.Unicorn, 1)]
        [TestCase(Rarity.Unicorn, .85)]
        [TestCase(Rarity.Unicorn, .75)]
        [TestCase(Rarity.Unicorn, .5)]
        [TestCase(Rarity.Unicorn, .25)]
        public void RepairPart(Rarity rare, decimal durability)
        {
            // arrange
            var playerService = GenerateService(0);
            var prt = new Head()
            {
                PartDurability = durability,
                PartRarity = rare
            };
            int scraps = 500;
            decimal remaining = 1 - durability;

            double cost = (double)(remaining) * StaticReference.PartsPerRarity[rare];

            int expected = scraps - (int)(cost);
            expected = expected > 0 ? expected : 0;
            decimal dura;
            if (cost <= scraps)
            {
                dura = 1;
            }
            else
            {
                dura = (scraps / (decimal)StaticReference.PartsPerRarity[rare]) + (durability);
            }

            // act
            var result = playerService.RepairPart(prt, scraps);

            // assert
            Assert.AreEqual(expected, result);
            Assert.AreEqual(dura, prt.PartDurability);
        }

        [Test]
        [TestCase(Structure.Animal)]
        [TestCase(Structure.Mechanical)]
        [TestCase(Structure.Magical)]
        [TestCase(Structure.Rock)]
        [TestCase(Structure.Human)]
        public void OrchestratePartRepair_UpdatesCount(Structure structureType)
        {
            // arrange
            var playerService = GenerateService(0);
            var player = new PlayerData() { Monster = new MonsterData() };
            player.SpareParts[structureType] = 10;
            player.Monster.Parts.Add(new Head()
            {
                PartRarity = Rarity.Common,
                PartStructure = structureType,
                PartDurability = 0
            });
            var expected = $"Common {structureType} head is now at 1 durability.\r\nYou now have 0 {structureType} parts.";

            // act
            var result = playerService.OrchestratePartRepair(player, 0);

            // assert
            Assert.AreEqual(0, player.SpareParts[structureType]);
            Assert.AreEqual(1, player.Monster.Parts[0].PartDurability);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(5, 0)]
        [TestCase(5, 5)]
        [TestCase(0, 5)]
        [TestCase(3, 3)]
        [TestCase(1, 9)]
        public void DumpBagIntoWorkshop_ReturnsCorrectCount(int bagStart, int workshopStart)
        {
            // arrange
            var playerService = GenerateService(0);
            var player = new PlayerData();
            for (int i = 0; i < workshopStart; i++)
            {
                player.WorkshopCuppoard.Add(new Head());
            }
            for (int i = 0; i < bagStart; i++)
            {
                player.Bag[i] =new Head();
            }
            var expected = bagStart + workshopStart;

            // act
            playerService.DumpBagIntoWorkshop(player);
            var bagCount = 0;
            foreach (var item in player.Bag)
            {
                if(item != null) bagCount++;
            }

            // assert
            Assert.AreEqual(expected, player.WorkshopCuppoard.Count);
            Assert.AreEqual(0, bagCount);
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(25)]
        [TestCase(0)]
        public void GetCheckWorkshopItemList_ReturnsCorrectString(int workshopCount)
        {
            // arrange
            var playerService = GenerateService(0);
            var player = new PlayerData();
            var expected = "Workshop Items:\r\n";
            for (int i = 0; i < workshopCount; i++)
            {
                expected += $"{i+1} - Common Animal head\r\n";
                player.WorkshopCuppoard.Add(new Head()
                {
                    PartRarity = Rarity.Common,
                    PartStructure = Structure.Animal
                });
            }
            if (workshopCount == 0)
            {
                expected += "Workshop cuppboard is empty\r\n";
            }

            // act
            var result = playerService.GetWorkshopItemList(player);

            // assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Compare_ByWins()
        {
            // arrange
            var playerService = GenerateService(0);
            var winner = new PlayerData()
            {
                WinsCount = 2,
                FightsCount = 1,
                Name = "Bertram"
            };
            var loser = new PlayerData()
            {
                WinsCount = 1,
                FightsCount = 2,
                Name = "Aardvark"
            };

            // act
            var result1 = playerService.Compare(winner, loser);
            var result2 = playerService.Compare(loser, winner);

            // assert
            Assert.AreEqual(1, result1);
            Assert.AreEqual(-1, result2);
        }

        [Test]
        public void Compare_ByFights()
        {
            // arrange
            var playerService = GenerateService(0);
            var winner = new PlayerData()
            {
                WinsCount = 2,
                FightsCount = 1,
                Name = "Bertram"
            };
            var loser = new PlayerData()
            {
                WinsCount = 2,
                FightsCount = 2,
                Name = "Aardvark"
            };

            // act
            var result1 = playerService.Compare(winner, loser);
            var result2 = playerService.Compare(loser, winner);

            // assert
            Assert.AreEqual(1, result1);
            Assert.AreEqual(-1, result2);
        }

        [Test]
        public void Compare_ByName()
        {
            // arrange
            var playerService = GenerateService(0);
            var winner = new PlayerData()
            {
                WinsCount = 2,
                FightsCount =2,
                Name = "Aardvark"
            };
            var loser = new PlayerData()
            {
                WinsCount = 2,
                FightsCount = 2,
                Name = "Bertram"
            };

            // act
            var result1 = playerService.Compare(winner, loser);
            var result2 = playerService.Compare(loser, winner);

            // assert
            Assert.AreEqual(1, result1);
            Assert.AreEqual(-1, result2);
        }

        [Test]
        public void Compare_AreEqual()
        {
            // arrange
            var playerService = GenerateService(0);
            var winner = new PlayerData()
            {
                WinsCount = 2,
                FightsCount = 2,
                Name = "Aardvark"
            };
            var loser = new PlayerData()
            {
                WinsCount = 2,
                FightsCount = 2,
                Name = "Aardvark"
            };

            // act

            // act
            var result1 = playerService.Compare(winner, loser);
            var result2 = playerService.Compare(loser, winner);

            // assert
            Assert.AreEqual(0, result1);
            Assert.AreEqual(0, result2);
        }
    }
}
