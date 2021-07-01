using System.Collections.Generic;

// todo: Probably not the right namespace for this
namespace doctor_mangle.models.parts
{     
    public class PartComparer : IComparer<BodyPart>
    {
        public int Compare(BodyPart x, BodyPart y)
        {
            if (x == null && y != null)
            {
                return 1;
            }
            else if (x != null && y == null)
            {
                return -1;
            }
            else if (x == null && y == null)
            {
                return 0;
            }
            else if (x.PartStructure.CompareTo(y.PartStructure) != 0)
            {
                return x.PartStructure.CompareTo(y.PartStructure);
            }
            else if (x.PartRarity.CompareTo(y.PartRarity) != 0)
            {
                return x.PartRarity.CompareTo(y.PartRarity);
            }
            else if (x.GetType() == typeof(PairedBodyPart) && y.GetType() == typeof(PairedBodyPart))
            {
                var x1 = (PairedBodyPart)x;
                var y1 = (PairedBodyPart)y;
                return x1.IsLeftSide.CompareTo(y1.IsLeftSide);
            }
            else
            {
                return 0;
            }
        }

        ///////////////////////////equality implementation if I ever need to do non-reference equality
        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        //public override string ToString()
        //{
        //    return base.ToString();
        //}
    }
}
