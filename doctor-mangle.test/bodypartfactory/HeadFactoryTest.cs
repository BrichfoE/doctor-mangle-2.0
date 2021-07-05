using doctor_mangle.constants;
using doctor_mangle.models.factories;
using doctor_mangle.models.parts;
using NUnit.Framework;
using System.Collections.Generic;

namespace doctor_mangle.test.bodypartfactory
{
    [TestFixture]
    public class HeadFactoryTest
    {
        private BodyPart _head;
        private HeadFactory _headFactory;

        private void Setup(Structure? structure)
        {
            _headFactory = new HeadFactory(structure);
            _head = _headFactory.BodyPart;
        }

        private void Setup()
        {
            Setup(null);
        }








        [TearDown]
        public void CleanUp()
        {
            _headFactory = null;
            _head = null;
        }

        [Test]
        public void HeadFactory_PartType_IsCorrectType()
        {
            // arrange
            Setup();

            // act
            Part result = _head.PartType;

            // assert







            Assert.IsNotNull(result);
            Assert.AreEqual(Part.head, result);
        }

        [Test]
        public void HeadFactory_IsLeftSide_IsNull()
        {
            // arrange
            Setup();

            // act
            bool? result = _head.IsLeftSide;

            // assert







            Assert.IsNull(result);
        }

        [Test]
        public void HeadFactory_PartDurability_IsNotNull_Equals1()
        {
            // arrange
            Setup();
            decimal expected = 1;

            // act
            // _head.GenerateBodyPart(null);
            decimal result = _head.PartDurability;

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
        public void HeadFactory_PartStructure_IsNotNull_EqualsExpected(Structure? structure)
        {
            // arrange
            Setup(structure);

            // act
            // _head.GenerateBodyPart(expected);
            Structure result = _head.PartStructure;

            // assert
            Assert.IsNotNull(result);
            if (structure != null)
            {
                Assert.AreEqual(structure, result);
            }
        }

        [Test]
        [Repeat(5)]
        public void HeadFactory_PartRarity_IsNotNull_BodyPart()
        {
            // arrange
            Setup();

            // act
            Rarity result = _head.PartRarity;

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
        public void HeadFactory_StatsMinMax(Structure structure)
        {
            // arrange
            Setup(structure);

            var rarityMult = StaticReference.RarityMultiplier[_head.PartRarity];

            // act
            Dictionary<Stat, float> result = _head.PartStats;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            Assert.GreaterOrEqual(result[Stat.Alacrity]/rarityMult, .55f, "Alacrity");
            Assert.GreaterOrEqual(result[Stat.Strength]/rarityMult, .22f, "Strength");
            Assert.GreaterOrEqual(result[Stat.Endurance]/rarityMult, .36666f, "Endurance");
            Assert.GreaterOrEqual(result[Stat.Technique]/rarityMult, 7f, "Technique");

            Assert.LessOrEqual(result[Stat.Alacrity]/rarityMult, 21f, "Alacrity");
            Assert.LessOrEqual(result[Stat.Strength]/rarityMult, 6.3f, "Strength");
            Assert.LessOrEqual(result[Stat.Endurance]/rarityMult, 14f, "Endurance");
            Assert.LessOrEqual(result[Stat.Technique]/rarityMult, 48f, "Technique");
        }
    }
}
