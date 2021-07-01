namespace doctor_mangle.models.parts
{
    public abstract class PairedBodyPart : BodyPart
    {
        public bool IsLeftSide { get; set; }
        public override string PartName
        {
            get
            {
                var side = IsLeftSide ? "left" : "right";
                return $"{PartRarity} {PartStructure} {side} {PartType}";
            }
        }
    }
}
