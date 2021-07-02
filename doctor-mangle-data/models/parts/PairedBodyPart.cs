namespace doctor_mangle.models.parts
{
    public abstract class PairedBodyPart : BodyPart
    {
        public override string PartName
        {
            get
            {
                var side = IsLeftSide != null 
                    ? IsLeftSide.Value
                        ? "left" 
                        : "right"
                    : "";
                return $"{PartRarity} {PartStructure} {side} {PartType}";
            }
        }
    }
}
