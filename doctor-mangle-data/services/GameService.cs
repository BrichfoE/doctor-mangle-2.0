using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doctor_mangle
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

        // deprecated - used to limit to 6 parts, need to rethink how to keep players from adding infinite parts for best monster
        private void OldAIBuildTurn(GameData data)
        {
            foreach (var ai in data.AiPlayers)
            {
                int start = 0;
                var monst = new BodyPart[6];
                if (ai.Monster != null)
                {
                    bool betterBody = false;
                    List<BodyPart> heads = ai.WorkshopCuppoard
                        .Where(x => x.PartType == Part.head
                            && x.PartRarity > ai.Monster.Parts[0].PartRarity)
                        .ToList();
                    List<BodyPart> torsos = ai.WorkshopCuppoard
                        .Where(x => x.PartType == Part.torso
                            && x.PartRarity > ai.Monster.Parts[1].PartRarity).
                        ToList();
                    if (heads.Count > 0 || torsos.Count > 0) betterBody = true;

                    if (betterBody)
                    {
                        data.Graveyard.Add(new MonsterGhost(ai.Monster, data.GameDayNumber));
                        for (int i = 2; i < ai.Monster.Parts.Count; i++)
                        {
                            if(ai.Monster.Parts[i] != null) ai.WorkshopCuppoard.Add(ai.Monster.Parts[i]);
                        }
                        ai.Monster = null;
                        ai.WorkshopCuppoard.Sort(_partComparer);
                    }
                    else
                    {
                        for (int i = 0; i < ai.Monster.Parts.Count; i++)
                        {
                            monst[i] = ai.Monster.Parts[i];
                        }
                        start = 2;
                    }
                }
                // replace any limbs with better parts
                for (int i = start; i < 6; i++)
                {
                    for (int j = ai.WorkshopCuppoard.Count - 1; j >= 0; j--)
                    {
                        BodyPart oldP = monst[i];
                        BodyPart newP = ai.WorkshopCuppoard[j];
                        float score = 0;

                        if (newP != null)
                        {
                            if (oldP != null && newP.PartType == (Part)i)
                            {
                                score += newP.PartStats[Stat.Alacrity] - monst[i].PartStats[Stat.Alacrity];
                                score += newP.PartStats[Stat.Strength] - monst[i].PartStats[Stat.Strength];
                                score += newP.PartStats[Stat.Endurance] - monst[i].PartStats[Stat.Endurance];
                                score += newP.PartStats[Stat.Technique] - monst[i].PartStats[Stat.Technique];
                            }
                            if ((oldP == null || score > 0f) && newP.PartType == (Part)i)
                            {
                                monst[i] = newP;
                            }
                        }
                    }
                }
                // ensure ai has a viable monster
                if (monst[0] != null && monst[1] != null)
                {
                    if (monst[2] != null || monst[3] != null || monst[4] != null || monst[5] != null)
                    {
                        if (ai.Monster == null)
                        {
                            ai.Monster = new MonsterData(ai.Name + "'s Monster", monst);
                        }
                        else
                        {
                            for (int i = 0; i < monst.Length; i++)
                            {
                                if (ai.Monster.Parts.Count > i)
                                {
                                    ai.Monster.Parts[i] = monst[i];
                                }
                                else if (monst[i] != null)
                                {
                                    ai.Monster.Parts.Add(monst[i]);
                                }
                            }
                        }
                        for (int i = ai.WorkshopCuppoard.Count - 1; i >= 0; i--)
                        {
                            if (ai.WorkshopCuppoard[i] != null)
                            {
                                _playerService.ScrapItem(ai.SpareParts, ai.WorkshopCuppoard, i);
                            }
                        }
                    }
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
