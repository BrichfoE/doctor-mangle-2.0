using doctor_mangle.constants;
using System;

namespace doctor_mangle.utility
{
    public static class StaticUtility
    {
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

        public static void TalkPause(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey(true);
        }

        public static int CheckInput(int low, int high)
        {
            string strInput = "";
            int input = 0;
            bool halt = true;
            bool formatError = false;
            while (halt)
            {
                formatError = false;
                strInput = Console.ReadLine();
                if (strInput == null)
                {
                    Console.WriteLine("Please choose a number " + low + "-" + high);
                }
                else
                {
                    try
                    {
                        input = Convert.ToInt32(strInput);
                    }
                    catch (System.FormatException)
                    {
                        Console.WriteLine("Please choose a number " + low + "-" + high);
                        formatError = true;
                    }
                    if ((input < low || input > high) && formatError == false)
                    {
                        Console.WriteLine("Please choose a number " + low + "-" + high);
                    }
                    else if (formatError == false)
                    {
                        halt = false;
                    }
                }


            }

            return input;
        }
    }
}
    
