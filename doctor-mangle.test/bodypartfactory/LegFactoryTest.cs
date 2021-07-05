using doctor_mangle.constants;
using doctor_mangle.models.factories;
using doctor_mangle.models.parts;
using NUnit.Framework;
using System.Collections.Generic;

namespace doctor_mangle.test.bodypartfactory
{
    [TestFixture]
    public class LegFactoryTest
    {
        private BodyPart _leg;
        private LegFactory _legFactory;

        private void Setup(Structure? structure)
        {
            _legFactory = new LegFactory(structure);
            _leg = _legFactory.BodyPart;
        }

        private void Setup()
        {
            Setup(null);
        }

        [TearDown]
        public void CleanUp()
        {
            _legFactory = null;
            _leg = null;
        }

        [Test]
        public void PartType_IsCorrectType()
        {
            // arrange
            Setup();

            // act
            Part result = _leg.PartType;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Part.leg, result);
        }

        [Test]
        [Repeat(5)]
        public void IsLeft_IsNotNull_GeneratesName()
        {
            // arrange
            Setup();

            // act
            bool isLeft = _leg.IsLeftSide.Value;
            var expected = isLeft
                ? "left leg"
                : "right leg";
            string name = _leg.PartName;
            var result = name.Substring(name.Length - expected.Length, expected.Length);

            // assert
            Assert.IsNotNull(isLeft);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GenerateBodyPart_PartDurability_IsNotNull_Equals1()
        {
            // arrange - in initalize
            Setup();
            decimal expected = 1;

            // act
            decimal result = _leg.PartDurability;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(Structure.Magical)]
        [TestCase(Structure.Animal)]
        [TestCase(Structure.Human)]
        [TestCase(Structure.Mechanical)]
        [TestCase(Structure.Rock)]
        [TestCase(null)]
        public void GenerateBodyPart_PartStructure_IsNotNull_EqualsExpected(Structure? expected)
        {
            // arrange
            Setup(expected);

            // act
            Structure result = _leg.PartStructure;

            // assert
            Assert.IsNotNull(result);
            if (expected != null)
            {
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        [Repeat(5)]
        public void GenerateBodyPart_PartRarity_IsNotNull_BodyPart()
        {
            // arrange
            Setup();

            // act
            Rarity result = _leg.PartRarity;

            // assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual((Rarity)0, result);
            Assert.IsInstanceOf(typeof(Rarity), result);
        }

        [Test]
        [TestCase(Structure.Magical)]
        [TestCase(Structure.Animal)]
        [TestCase(Structure.Human)]
        [TestCase(Structure.Mechanical)]
        [TestCase(Structure.Rock)]
        [Repeat(25)]
        public void SetStats_PartRarity_IsNotNull_BodyPart(Structure structure)
        {
            // arrange - adjust base stats as we're testing the random range here
            Setup(structure);
            var rarityMult = StaticReference.RarityMultiplier[_leg.PartRarity];

            // act
            Dictionary<Stat, float> result = _leg.PartStats;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            Assert.GreaterOrEqual(result[Stat.Alacrity]/rarityMult, 15f, "Alacrity");
            Assert.GreaterOrEqual(result[Stat.Strength]/rarityMult, 2.75f, "Strength");
            Assert.GreaterOrEqual(result[Stat.Endurance]/rarityMult, .36f, "Endurance");
            Assert.GreaterOrEqual(result[Stat.Technique]/rarityMult, .22f, "Technique");

            Assert.LessOrEqual(result[Stat.Alacrity]/rarityMult, 50f, "Alacrity");
            Assert.LessOrEqual(result[Stat.Strength]/rarityMult, 21f, "Strength");
            Assert.LessOrEqual(result[Stat.Endurance]/rarityMult, 10.5f, "Endurance");
            Assert.LessOrEqual(result[Stat.Technique]/rarityMult, 4.2f, "Technique");
        }
    }
}
