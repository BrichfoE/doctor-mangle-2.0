﻿using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;

namespace doctor_mangle.Service
{
    public class PlayerService : IPlayerService, IComparer<PlayerData>
    {
        private readonly Random _rng;

        // todo: make BodyPartService, transfer this there
        private PartComparer _comparer = new PartComparer();
        public PartComparer Comparer { get => _comparer; }

        // todo: remove parameterless constructor once we fix the GameData/GameController mess
        public PlayerService() { _rng = new Random(); }
        public PlayerService(Random rng)
        {
            this._rng = rng;
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
            player.WorkshopCuppoard.Sort(this.Comparer);
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
    }
}
