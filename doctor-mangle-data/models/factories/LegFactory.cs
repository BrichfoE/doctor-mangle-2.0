using doctor_mangle.constants;
using doctor_mangle.models.parts;
using doctor_mangle.utility;
using System;

namespace doctor_mangle.models.factories
{
    public class LegFactory : PairedBodyPartFactory
    {
        public LegFactory(Structure? structure) : base(structure)
        {
        }

        public override void GenerateBodyPart(Structure? structure)
        {
            this._bodyPart = new Leg();
            Random _rng = new Random();
            this._bodyPart.PartDurability = 1;
            this._bodyPart.IsLeftSide = _rng.Next(1) == 1;

            // set stat base for leg
            this._bodyPart.PartStats[Stat.Alacrity] += .5f;
            this._bodyPart.PartStats[Stat.Strength] += .1f;
            this._bodyPart.PartStats[Stat.Endurance] += .1f;
            this._bodyPart.PartStats[Stat.Technique] += .1f;

            // add structure and stat bonus
            this._bodyPart.PartStructure = structure ?? (Structure)_rng.Next(1, 5);
            if (this._bodyPart.PartStructure == Structure.Magical)
            {
                this._bodyPart.PartStats[Stat.Alacrity] += .5f;
                this._bodyPart.PartStats[Stat.Strength] += .5f;
                this._bodyPart.PartStats[Stat.Endurance] += .5f;
                this._bodyPart.PartStats[Stat.Technique] += .5f;
            }
            else
            {
                this._bodyPart.PartStats[StaticReference.StructureAffinity[this._bodyPart.PartStructure]] += 1f;
            }

            // roll each stat
            this._bodyPart.PartStats[Stat.Alacrity] *= _rng.Next(10, 20);
            this._bodyPart.PartStats[Stat.Strength] *= _rng.Next(5, 20);
            this._bodyPart.PartStats[Stat.Endurance] *= _rng.Next(1, 15);
            this._bodyPart.PartStats[Stat.Technique] *= _rng.Next(1, 10);

            // get random rarity (if default) and apply regardless
            this._bodyPart.PartRarity = this._bodyPart.PartRarity == 0
                    ? StaticUtility.GetRarity(_rng.Next(1, 1000))
                    : this._bodyPart.PartRarity;
            this._bodyPart.PartStats[Stat.Alacrity] *= StaticReference.RarityMultiplier[this._bodyPart.PartRarity] / 1f;
            this._bodyPart.PartStats[Stat.Strength] *= StaticReference.RarityMultiplier[this._bodyPart.PartRarity] / 2f;
            this._bodyPart.PartStats[Stat.Endurance] *= StaticReference.RarityMultiplier[this._bodyPart.PartRarity] / 3f;
            this._bodyPart.PartStats[Stat.Technique] *= StaticReference.RarityMultiplier[this._bodyPart.PartRarity] / 5f;
        }
    }
}
