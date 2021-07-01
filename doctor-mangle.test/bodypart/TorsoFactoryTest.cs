using doctor_mangle.constants;
using doctor_mangle.models.factories;
using doctor_mangle.models.parts;
using NUnit.Framework;
using System.Collections.Generic;

namespace doctor_mangle.bodypart
{
    [TestFixture]
    public class TorsoFactoryTest
    {
        private BodyPart _torso;
        private TorsoFactory _torsoFactory;

        private void Setup(Structure? structure)
        {
            _torsoFactory = new TorsoFactory(structure);
            _torso = _torsoFactory.BodyPart;
        }

        private void Setup()
        {
            Setup(null);
        }

        [TearDown]
        public void CleanUp()
        {
            _torsoFactory = null;
            _torso = null;
        }

        [Test]
        public void PartType_IsCorrectType()
        {
            // arrange
            Setup();

            // act
            Part result = _torso.PartType;

            // assert          
            Assert.IsNotNull(result);
            Assert.AreEqual(Part.torso, result);
        }

        [Test]
        public void GenerateBodyPart_PartDurability_IsNotNull_Equals1()
        {
            // arrange 
            Setup();
            decimal expected = 1;

            // act
            decimal result = _torso.PartDurability;

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
            Structure result = _torso.PartStructure;

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
            Rarity result = _torso.PartRarity;

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
            var rarityMult = StaticReference.RarityMultiplier[_torso.PartRarity];

            // act
            Dictionary<Stat, float> result = _torso.PartStats;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            Assert.GreaterOrEqual(result[Stat.Alacrity]/rarityMult, .36f, "Alacrity");
            Assert.GreaterOrEqual(result[Stat.Strength]/rarityMult, 2.75f, "Strength");
            Assert.GreaterOrEqual(result[Stat.Endurance]/rarityMult, 18f, "Endurance");
            Assert.GreaterOrEqual(result[Stat.Technique]/rarityMult, .22f, "Technique");

            Assert.LessOrEqual(result[Stat.Alacrity]/rarityMult, 10.5f, "Alacrity");
            Assert.LessOrEqual(result[Stat.Strength]/rarityMult, 21f, "Strength");
            Assert.LessOrEqual(result[Stat.Endurance]/rarityMult, 56f, "Endurance");
            Assert.LessOrEqual(result[Stat.Technique]/rarityMult, 6.3f, "Technique");
        }
    }
}
