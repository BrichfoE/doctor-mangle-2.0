using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doctor_mangle.services
{
    public class PlayerService : IPlayerService, IComparer<PlayerData>
    {
        private readonly Random _rng;
        private readonly IComparer<BodyPart> _comparer;

        public PlayerService(IComparer<BodyPart> partComparer, Random random)
        {
            this._rng = random;
            this._comparer = partComparer;
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
            storage.Sort(this._comparer);

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

        public void DumpBagIntoWorkshop(PlayerData player)
        {
            for (int i = 0; i < player.Bag.Length; i++)
            {
                if (player.Bag[i] != null)
                {
                    player.WorkshopCuppoard.Add(player.Bag[i]);
                    player.Bag[i] = null;
                }
            }
            player.WorkshopCuppoard.Sort(this._comparer);
        }

        public string GetWorkshopItemList(PlayerData player)
        {
            string result = "Workshop Items:\r\n";
            int count = 1;
            foreach (var part in player.WorkshopCuppoard)
            {
                if (part != null)
                {
                    result += $"{count} - {part.PartName}\r\n";
                    count += 1;
                }
            }
            if (count == 1)
            {
                result += "Workshop cuppboard is empty\r\n";
            }
            return result;
        }
        public PlayerData[] SortPlayersByWins(PlayerData[] players)
        {
            for (int i = 0; i < players.Length; i++)
            {
                PlayerData left = players[i];
                PlayerData high = players[i];
                int highIndex = i;

                for (int j = i + 1; j < players.Length; j++)
                {
                    if (Compare(high, players[j]) < 0)
                    {
                        high = players[j];
                        highIndex = j;
                    }
                }

                if (left != high)
                {
                    players[highIndex] = left;
                    players[i] = high;
                }
            }
            return players;
        }

        public int Compare(PlayerData x, PlayerData y)
        {
            // more wins equals earlier in order
            var result = x.WinsCount.CompareTo(y.WinsCount);
            if (result != 0) return result;

            // fewer fights equals earlier in order (because higher win percentage)
            result = y.FightsCount.CompareTo(x.FightsCount);
            if (result != 0) return result;

            // earlier in alphabet equals earlier in order
            result = y.Name.CompareTo(x.Name);
            if (result != 0) return result;

            return 0;
        }


        public void MovePartsForSerilaization<T>(T objectWithCollection)
        {
            if (objectWithCollection.GetType() != typeof(PlayerData))
            {
                throw new ArgumentException("Must use player in PlayerService serialization implementation");
            }

            var player = objectWithCollection as PlayerData;

            player._heads = player.WorkshopCuppoard
                .Where(x => x.GetType() == typeof(Head))
                .Select(y => new Head(y))
                .ToList();

            player._torsos = player.WorkshopCuppoard
                .Where(x => x.GetType() == typeof(Torso))
                .Select(y => new Torso(y))
                .ToList();

            player._arms = player.WorkshopCuppoard
                .Where(x => x.GetType() == typeof(Arm))
                .Select(y => new Arm(y))
                .ToList();

            player._legs = player.WorkshopCuppoard
                .Where(x => x.GetType() == typeof(Leg))
                .Select(y => new Leg(y))
                .ToList();

            player.WorkshopCuppoard.Clear();
        }

        public void MovePartsAfterDeserilaization<T>(T objectWithCollection)
        {
            if (objectWithCollection.GetType() != typeof(PlayerData))
            {
                throw new ArgumentException("Must use player in PlayerService serialization implementation");
            }

            var player = objectWithCollection as PlayerData;

            foreach (var item in player._heads)
            {
                player.WorkshopCuppoard.Add(item);
            }
            player._heads.Clear();
            foreach (var item in player._torsos)
            {
                player.WorkshopCuppoard.Add(item);
            }
            player._torsos.Clear();
            foreach (var item in player._arms)
            {
                player.WorkshopCuppoard.Add(item);
            }
            player._arms.Clear();
            foreach (var item in player._legs)
            {
                player.WorkshopCuppoard.Add(item);
            }
            player._legs.Clear();
        }

        public void TryBuildMonster(PlayerData player)
        {
            var monst = new List<BodyPart>();

            if (player.Monster != null)
            {
                player.WorkshopCuppoard.AddRange(player.Monster.Parts);
                player.Monster.Parts.Clear();
            }

            bool hasHead = false;
            bool hasTorso = false;
            for (int i = player.WorkshopCuppoard.Count - 1; i >= 0; i--)
            {
                var item = player.WorkshopCuppoard[i];
                if (item.PartDurability > 0)
                {
                    monst.Add(item);
                    hasHead = hasHead || item.GetType() == typeof(Head);
                    hasTorso = hasTorso || item.GetType() == typeof(Torso);
                }
                else
                {
                    this.ScrapItem(player.SpareParts, player.WorkshopCuppoard, i);
                }
            }
            // ensure ai has a viable monster
            if (hasHead && hasTorso && monst.Count > 2)
            {
                if (player.Monster == null)
                {
                    player.Monster = new MonsterData(player.Name + "'s Monster", monst);
                }
                else
                {
                    player.Monster.Parts.AddRange(monst);
                }
                player.WorkshopCuppoard.Clear();
            }
        }
    }
}
