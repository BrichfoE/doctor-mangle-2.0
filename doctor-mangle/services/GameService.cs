using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;

namespace doctor_mangle.services
{
    public class GameService : IGameService
    {
        private readonly IParkService _parkService;
        private readonly IPlayerService _playerService;
        private readonly IComparer<BodyPart> _partComparer;
        private readonly Random _rng;

        public GameService(
            IPlayerService playerService,
            IParkService parkService,
            IComparer<BodyPart> partComparer,
            Random random)
        {
            this._parkService = parkService;
            this._playerService = playerService;
            this._partComparer = partComparer;
            this._rng = random;
        }

        public GameData GetNewGameData(string name, int aiCount, int gameID)
        {
            var ai = new PlayerData[aiCount];
            for (int i = 0; i < ai.Length; i++)
            {
                ai[i] = _playerService.GeneratePlayer("rando", true);
            }
            var data = new GameData()
            {
                GameDataId = gameID,
                GameName = name,
                Parks = _parkService.GenerateParks(),
                CurrentPlayer = _playerService.GeneratePlayer(name, false),
                AiPlayers = ai,
                Graveyard = new List<MonsterGhost>()
            };
            data.Parks = _parkService.AddParts(data.Parks, aiCount + 1);
            return data;
        }

        public string PrintRegionOptions(GameData gameData)
        {
            string result = string.Empty;
            for (int i = 1; i < 5; i++)
            {
                if (gameData.CurrentRegion == i)
                {
                    result += $"{i} - Stay in the {gameData.Parks[i].ParkName} \r\n";
                }
                else
                {
                    result += $"{i} - Go to the {gameData.Parks[i].ParkName} \r\n";
                }
            }
            return result;
        }

        // TODO: this needs to be refactored after I work out how to discourage just adding parts forever
        public void AIBuildTurn(GameData data)
        {
            foreach (var ai in data.AiPlayers)
            {
                var monst = new List<BodyPart>();

                if (ai.Monster != null)
                {
                    ai.WorkshopCuppoard.AddRange(ai.Monster.Parts);
                    ai.Monster.Parts.Clear();
                }

                bool hasHead = false;
                bool hasTorso = false;
                for (int i = ai.WorkshopCuppoard.Count-1; i >= 0; i--)
                {
                    var item = ai.WorkshopCuppoard[i];
                    if (item.PartDurability > 0)
                    {
                        monst.Add(item);
                        hasHead = hasHead || item.GetType() == typeof(Head);
                        hasTorso = hasTorso || item.GetType() == typeof(Torso);
                    }
                    else
                    {
                        _playerService.ScrapItem(ai.SpareParts, ai.WorkshopCuppoard, i);
                    }
                }
                // ensure ai has a viable monster
                if (hasHead && hasTorso && monst.Count > 2)
                {
                    if (ai.Monster == null)
                    {
                        ai.Monster = new MonsterData(ai.Name + "'s Monster", monst);
                    }
                    else
                    {
                        ai.Monster.Parts.AddRange(monst);
                    }
                    ai.WorkshopCuppoard.Clear();
                }
            }
        }

        public void AISearchTurn(GameData gd, int round)
        {
            foreach (var ai in gd.AiPlayers)
            {
                int region = _rng.Next(1, 4);
                if (gd.Parks[region].PartsList.Count != 0)
                {
                    ai.Bag[round - 1] = gd.Parks[region].PartsList.Last.Value;
                    gd.Parks[region].PartsList.RemoveLast();
                }
            }
        }
    }
}
