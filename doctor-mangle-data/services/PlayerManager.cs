using doctor_mangle.constants;
using doctor_mangle.models;
using doctor_mangle.models.parts;
using doctor_mangle.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMangle.Service
{
    public class PlayerManager
    {

        public void CheckBag(PlayerData player)
        {
            if (!player.IsAI)
            {
                int counter = 1;
                foreach (var part in player.Bag)
                {
                    if (part != null)
                    {
                        Console.WriteLine(counter + " - " + part.PartName);
                        counter = counter + 1;
                    }
                }
            }
            else
            {
                Console.WriteLine("Hands Off!");
            }
        }

        public void ScrapItem(PlayerData player, List<BodyPart> storage, int reference)
        {
            BodyPart part = storage[reference];
            Random r = new Random();
            int high = 2;
            int amount = 1;

            switch (part.PartRarity)
            {
                case Rarity.Unicorn:
                    high = 1000;
                    break;
                case Rarity.Mythic:
                    high = 500;
                    break;
                case Rarity.Legendary:
                    high = 200;
                    break;
                case Rarity.Epic:
                    high = 100;
                    break;
                case Rarity.Rare:
                    high = 50;
                    break;
                case Rarity.Common:
                    high = 10;
                    break;
                default:
                    throw new Exception("Cannot Scrap Unknown PartRarity");
            }

            amount = (r.Next(high) * (Int32)(part.PartDurability * 100)) / 100;

            player.ComponentList[Convert.ToInt32(part.PartStructure)] += amount;

            storage.RemoveAt(reference);
            if (!player.IsAI)
            {
                Console.WriteLine($"You salvaged {amount} {part.PartStructure} parts.");
            }

            DumpWorkshopNulls(player);
            storage.Sort(player.Comparer);
        }

        public void RepairMonster(PlayerData player, int reference)
        {
            BodyPart part = player.Monster.Parts[reference];

            int full = 2;
            switch (part.PartRarity)
            {
                case Rarity.Unicorn:
                    full = 1000;
                    break;
                case Rarity.Mythic:
                    full = 500;
                    break;
                case Rarity.Legendary:
                    full = 200;
                    break;
                case Rarity.Epic:
                    full = 100;
                    break;
                case Rarity.Rare:
                    full = 50;
                    break;
                case Rarity.Common:
                    full = 10;
                    break;
                default:
                    throw new Exception("Cannot Repair Unknown PartRarity");
            }

            int cost = ((Int32)((1 - part.PartDurability) * 100) * full) / 100;
            if (cost < 0) cost = 0;
            int intInput = 1;
            int structureInt = Convert.ToInt32(part.PartStructure);

            if (!player.IsAI)
            {
                Console.WriteLine($"Full repair will cost {cost} {part.PartStructure} parts. You currently have {player.ComponentList[structureInt]}.");
                Console.WriteLine("Confirm repair?");
                Console.WriteLine("1 - Yes");
                Console.WriteLine("2 - No");
                intInput = StaticUtility.CheckInput(1, 2);
            }
            if (intInput == 1)
            {
                if (cost <= player.ComponentList[structureInt])
                {
                    player.ComponentList[Convert.ToInt32(part.PartStructure)] -= cost;
                    part.PartDurability = 1;
                }
                else
                {  //This could be stated in two lines, but this was easier to debug
                    decimal percentage = ((decimal)player.ComponentList[structureInt] / cost);
                    decimal remaining = (1 - part.PartDurability);
                    part.PartDurability +=  remaining * percentage;
                    player.ComponentList[structureInt] = 0;
                }

                if (!player.IsAI)
                {
                    Console.WriteLine(part.PartName + " is now at " + part.PartDurability + " durability.");
                    Console.WriteLine($"You now have {player.ComponentList[structureInt]} {part.PartStructure} parts.");
                }
            }
        }

        public void DumpWorkshopNulls(PlayerData player)
        {
            player.Workshop = player.Workshop.Where(x => x != null).ToList();
        }

        public void DumpBag(PlayerData player)
        {
            for (int i = 0; i < player.Bag.Length; i++)
            {
                if (player.Bag[i] != null)
                {
                    player.Workshop.Add(player.Bag[i]);
                    player.Bag[i] = null;
                }
            } 

            player.Workshop.Sort(player.Comparer);
        }

        public void CheckWorkshop(PlayerData player)
        {
            if (!player.IsAI)
            {
                player.Workshop.Sort(player.Comparer);
                Console.WriteLine("Workshop Items:");
                int count = 1;
                foreach (var part in player.Workshop)
                {
                    if (part != null)
                    {
                        Console.WriteLine(count + " - " + part.PartName);
                        count += 1;
                    }
                }
            }
        }

        

    }
    
}
