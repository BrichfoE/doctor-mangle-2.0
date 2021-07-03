using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.parts;
using doctor_mangle.utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doctor_mangle.Service
{
    public class PlayerService : IPlayerService, IComparer<PlayerData>
    {
        private readonly Random _rng;
        // todo: remove parameterless constructor once we get depdency injection going
        public PlayerService() { }
        public PlayerService(Random rng)
        {
            _rng = rng;
        }
        
        private PartComparer _comparer = new PartComparer();
        public PartComparer Comparer { get => _comparer; }
        public PlayerData GeneratePlayer(string playerName, bool isAI)
        {
            var _player = new PlayerData();
            _player.IsAI = isAI;

            if (isAI || string.IsNullOrEmpty(playerName))
            {
                _player.Name = this.GenerateRandomName();
            }
            else
            {
                _player.Name = (string)playerName;
            }

            return _player;
        }

        public string GenerateRandomName()
        {
            // todo: come back and add a chance to have epithets insetead of adjectives

            int adjInt = (int)(_rng.NextDouble() * (double)(StaticReference.adjectives.Length-1));
            int namInt = (int)(_rng.NextDouble() * (double)(StaticReference.names.Length-1));

            return StaticReference.adjectives[adjInt] + " " + StaticReference.names[namInt];
        }

        public string CheckBag(PlayerData player)
        {
            string response = string.Empty;
            if (!player.IsAI)
            {
                int counter = 1;
                foreach (var part in player.Bag)
                {
                    if (part != null)
                    {
                        response += counter.ToString() + " - " + part.PartName + "\r\n";
                        counter = counter + 1;
                    }
                }
            }
            else
            {
                response = "Hands off!";
            }
            return response;
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

            player.SpareParts[part.PartStructure] += amount;

            storage.RemoveAt(reference);
            if (!player.IsAI)
            {
                Console.WriteLine($"You salvaged {amount} {part.PartStructure} parts.");
            }

            DumpWorkshopNulls(player);
            storage.Sort(this.Comparer);
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

            if (!player.IsAI)
            {
                Console.WriteLine($"Full repair will cost {cost} {part.PartStructure} parts. You currently have {player.SpareParts[part.PartStructure]}.");
                Console.WriteLine("Confirm repair?");
                Console.WriteLine("1 - Yes");
                Console.WriteLine("2 - No");
                intInput = StaticUtility.CheckInput(1, 2);
            }
            if (intInput == 1)
            {
                if (cost <= (int)player.SpareParts[part.PartStructure])
                {
                    player.SpareParts[part.PartStructure] -= cost;
                    part.PartDurability = 1;
                }
                else
                {  //This could be stated in two lines, but this was easier to debug
                    decimal percentage = ((decimal)player.SpareParts[part.PartStructure] / cost);
                    decimal remaining = (1 - part.PartDurability);
                    part.PartDurability +=  remaining * percentage;
                    player.SpareParts[part.PartStructure] = 0;
                }

                if (!player.IsAI)
                {
                    Console.WriteLine(part.PartName + " is now at " + part.PartDurability + " durability.");
                    Console.WriteLine($"You now have {player.SpareParts[part.PartStructure]} {part.PartStructure} parts.");
                }
            }
        }

        public void DumpWorkshopNulls(PlayerData player)
        {
            player.WorkshopCuppoard = player.WorkshopCuppoard.Where(x => x != null).ToList();
        }

        public void DumpBag(PlayerData player)
        {
            for (int i = 0; i < player.Bag.Length; i++)
            {
                if (player.Bag[i] != null)
                {
                    player.WorkshopCuppoard.Add(player.Bag[i]);
                    player.Bag[i] = null;
                }
            }

            player.WorkshopCuppoard.Sort(this.Comparer);
        }

        public void CheckWorkshop(PlayerData player)
        {
            if (!player.IsAI)
            {
                player.WorkshopCuppoard.Sort(this.Comparer);
                Console.WriteLine("Workshop Items:");
                int count = 1;
                foreach (var part in player.WorkshopCuppoard)
                {
                    if (part != null)
                    {
                        Console.WriteLine(count + " - " + part.PartName);
                        count += 1;
                    }
                }
            }
        }

        public int Compare(PlayerData x, PlayerData y)
        {
            if (x.WinsCount.CompareTo(y.WinsCount) != 0)
            {
                return x.WinsCount.CompareTo(y.WinsCount);
            }
            else if (x.FightsCount.CompareTo(y.FightsCount) != 0)
            {
                return x.FightsCount.CompareTo(y.FightsCount);
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
