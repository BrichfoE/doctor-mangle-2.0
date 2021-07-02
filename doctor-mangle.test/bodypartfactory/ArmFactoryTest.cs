using doctor_mangle.constants;
using doctor_mangle.models.factories;
using doctor_mangle.models.parts;
using NUnit.Framework;
using System.Collections.Generic;

namespace doctor_mangle.bodypartfactory
{
    [TestFixture]
    public class ArmFactoryTest
    {
        private BodyPart _arm;
        private ArmFactory _armFactory;

        private void Setup(Structure? structure)
        {
            _armFactory = new ArmFactory(structure);
            _arm = _armFactory.BodyPart;
        }

        private void Setup()
        {
            Setup(null);
        }

        [TearDown]
        public void CleanUp()
        {
            _armFactory = null;
            _arm = null;
        }

        [Test]
        public void PartType_IsCorrectType()
        {
            // arrange
            Setup();

            // act
            Part result = _arm.PartType;

            // assert          
            Assert.IsNotNull(result);
            Assert.AreEqual(Part.arm, result);
        }

        [Test]
        [Repeat(5)]
        public void IsLeft_IsNotNull_GeneratesName()
        {
            // arrange
            Setup();

            // act
            bool isLeft = _arm.IsLeftSide.Value;
            var expected = isLeft
                ? "left arm"
                : "right arm";
            string name = _arm.PartName;
            var result = name.Substring(name.Length - expected.Length, expected.Length);

            // assert          
            Assert.IsNotNull(isLeft);
            Assert.AreEqual(expected, result);            
        }
        
        [Test]
        public void GenerateBodyPart_PartDurability_IsNotNull_Equals1()
        {
            // arrange
            Setup();
            decimal expected = 1;

            // act
            decimal result = _arm.PartDurability;

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
        public void GenerateBodyPart_PartStructure_IsNotNull_EqualsExpected(Structure? structure)
        {
            // arrange - in initalize
            Setup(structure);

            // act
            Structure result = _arm.PartStructure;

            // assert
            Assert.IsNotNull(result);
            if (structure != null)
            {
                Assert.AreEqual(structure, result);
            }
        }

        [Test]
        [Repeat(5)]
        public void GenerateBodyPart_PartRarity_IsNotNull_BodyPart()
        {
            // arrange - in initalize

            // act
            Setup();
            Rarity result = _arm.PartRarity;

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
            // arrange
            Setup(structure);

            var rarityMult = StaticReference.RarityMultiplier[_arm.PartRarity];

            // act
            Dictionary<Stat, float> result = _arm.PartStats;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            Assert.GreaterOrEqual(result[Stat.Alacrity]/rarityMult, 3f, "Alacrity");
            Assert.GreaterOrEqual(result[Stat.Strength]/rarityMult, 15f, "Strength");
            Assert.GreaterOrEqual(result[Stat.Endurance]/rarityMult, .36f, "Endurance");
            Assert.GreaterOrEqual(result[Stat.Technique]/rarityMult, .22f, "Technique");

            Assert.LessOrEqual(result[Stat.Alacrity]/rarityMult, 22f, "Alacrity");
            Assert.LessOrEqual(result[Stat.Strength]/rarityMult, 50f, "Strength");
            Assert.LessOrEqual(result[Stat.Endurance]/rarityMult, 10.5f, "Endurance");
            Assert.LessOrEqual(result[Stat.Technique]/rarityMult, 4.2f, "Technique");
        }
    }
}
