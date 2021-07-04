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

        // todo: make BodyPartService, transfer this there
        private PartComparer _comparer = new PartComparer();
        public PartComparer Comparer { get => _comparer; }

        // todo: remove parameterless constructor once we get depdency injection going
        public PlayerService() { _rng = new Random(); }
        public PlayerService(Random rng)
        {
            _rng = rng;
        }
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

        public string ScrapItem(Dictionary<Structure, int> spareParts, List<BodyPart> storage, int reference)
        {
            BodyPart part = storage[reference];
            double max = StaticReference.PartsPerRarity[part.PartRarity];
            int amount = (int)(_rng.NextDouble() * max * (double)part.PartDurability);

            spareParts[part.PartStructure] += amount;

            string response = $"You salvaged {amount} {part.PartStructure} parts from a {part.PartName}.";

            storage.RemoveAt(reference);
            storage.Sort(this.Comparer);

            return response;
        }

        // todo: make BodyPartService, transfer this there
        public int GetRepairCost(BodyPart part)
        {
            decimal full = StaticReference.PartsPerRarity[part.PartRarity];
            int cost = (int)((1 - part.PartDurability) * full);
            return cost > 0
                ? cost
                : 0;
        }

        // todo: make BodyPartService, transfer this there
        public int RepairPart(BodyPart part, int availableParts)
        {
            decimal cost = this.GetRepairCost(part);
            var partsToReturn = availableParts;
            if (cost <= availableParts)
            {
                partsToReturn -= (int)cost;
                part.PartDurability = 1;
            }
            else
            {
                decimal percentage = ((decimal)availableParts / cost);
                decimal remaining = (1 - part.PartDurability);
                part.PartDurability += remaining * percentage;
                partsToReturn = 0;
            }

            return partsToReturn;
        }

        public string OrchestratePartRepair(PlayerData player, int reference)
        {
            BodyPart part = player.Monster.Parts[reference];
            player.SpareParts[part.PartStructure] = this.RepairPart(part, player.SpareParts[part.PartStructure]);

            return $"{part.PartName} is now at {part.PartDurability} durability.\r\nYou now have {player.SpareParts[part.PartStructure]} {part.PartStructure} parts.";
        }

        public void DumpWorkshopNulls(List<BodyPart> workshopCuppoard)
        {
            workshopCuppoard = workshopCuppoard.Where(x => x != null).ToList();
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
