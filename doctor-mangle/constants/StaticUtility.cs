using doctor_mangle.constants;
using System;
using System.Collections.Generic;

namespace doctor_mangle.utility
{
    public static class StaticUtility
    {
        public static int NonNullCount(IEnumerable<object> list)
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

        public static Rarity GetRarity(int rarityRoll)
        {
            if (rarityRoll < 500)
            {
                return Rarity.Common;
            }
            else if (rarityRoll < 750)
            {
                return Rarity.Rare;
            }
            else if (rarityRoll < 900)
            {
                return Rarity.Epic;
            }
            else if (rarityRoll < 980)
            {
                return Rarity.Legendary;
            }
            else if (rarityRoll < 999)
            {
                return Rarity.Mythic;
            }
            else
            {
                return Rarity.Unicorn;
            }
        }
    }
}

