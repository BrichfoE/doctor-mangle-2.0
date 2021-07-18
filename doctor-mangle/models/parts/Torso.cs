using doctor_mangle.constants;

namespace doctor_mangle.models.parts
{
    public class Torso : BodyPart
    {
        public override Part PartType { get => Part.torso; }

        public Torso() { }

        public Torso(BodyPart bodyPart)
        {
            PartType = bodyPart.PartType;
            PartStructure = bodyPart.PartStructure;
            PartRarity = bodyPart.PartRarity;
            PartDurability = bodyPart.PartDurability;
            IsLeftSide = bodyPart.IsLeftSide;
            PartStats[Stat.Alacrity] = bodyPart.PartStats[Stat.Alacrity];
            PartStats[Stat.Strength] = bodyPart.PartStats[Stat.Strength];
            PartStats[Stat.Endurance] = bodyPart.PartStats[Stat.Endurance];
            PartStats[Stat.Technique] = bodyPart.PartStats[Stat.Technique];
        }
    }
}
