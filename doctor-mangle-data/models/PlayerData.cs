namespace DrMangle
{
    using doctor_mangle.constants;
    using doctor_mangle.models;
    using doctor_mangle.models.parts;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PlayerData : IComparer<PlayerData>
    {
        public string Name { get; set; }

        public bool IsAI { get; set; }

        public int Wins { get; set; }

        public int Fights { get; set; }

        public MonsterData Monster { get; set; }

        public BodyPart[] Bag { get; set; }

        public List<BodyPart> Workshop { get; set; }

        public int Luck { get; set; }

        public int[] ComponentList { get; set; }

        public decimal Money { get; set; }

        internal PartComparer Comparer { get; set; }



        public PlayerData(string playerName, bool isAI)
        {
            if (isAI)
            {
                Random r = new Random();
                this.Name = this.RandomName(r.Next(1) * 10);  
            }
            else
            {
                this.Name = (string)playerName;
            }
            this.IsAI = isAI;
            this.Wins = 0;
            this.Fights = 0;
            this.Monster = null;
            this.Bag = new BodyPart[5];
            this.Workshop = new List<BodyPart>();
            this.ComponentList = new int[5];
            this.Comparer = new PartComparer();
        }

        private string RandomName(int input)
        {
            string result = string.Empty;
            Random r = new Random();

            int adjInt;
            int namInt;

            adjInt = (input * r.Next(1, 100)) % 10;
            namInt = (input * r.Next(1, 100)) % 10;

            result = StaticReference.adjectives[adjInt] + " " + StaticReference.names[namInt];

            return result;
        }



        internal int PartListCount(IEnumerable<BodyPart> list)
        {
            int count = 0;
            foreach (var item in list)
            {
                if (item != null)
                {
                    count += 1;
                }
            }
            return count;
        }

        public int Compare(PlayerData x, PlayerData y)
        {
            if (x.Wins.CompareTo(y.Wins) != 0)
            {
                return x.Wins.CompareTo(y.Wins);
            }
            else if (x.Fights.CompareTo(y.Fights) != 0)
            {
                return x.Fights.CompareTo(y.Fights);
            }
            else if (x.Name.CompareTo(y.Name) != 0)
            {
                return x.Name.CompareTo(y.Name);
            }
            else
            {
                return 0;
            }
        }

    }
}

  
