using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.battles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace doctor_mangle.services
{
    public class ArenaService : IArenaService
    {
        private readonly ILogger<IArenaService> _logger;
        private readonly IBattleService _battleService;
        public ArenaService (ILogger<IArenaService> logger, IBattleService battleService) 
        {
            this._logger = logger;
            this._battleService = battleService; 
        }
        public ArenaResult ManageBattlePhase(PlayerData[] players)
        {
            _logger.LogInformation("Beginning battle phase in Arena.");
            var result = new ArenaResult()
            {
                Fights = new List<BattleResult>(),
                ClosingLine = "We hope for a better show tomorrow!"
            };
            Queue<PlayerData> fighters = new Queue<PlayerData>();

            //find all available competitors
            foreach (var player in players)
            {
                if (player.Monster != null && player.Monster.CanFight)
                {
                    fighters.Enqueue(player);
                }
            }
            _logger.LogInformation($"Starting fights for {fighters.Count} monsters.");
            //pair off
            if (fighters.Count == 0)
            {
                result.OpeningLine = ("There will be no show tonight!  Better luck gathering tomorrow");
            }
            else if (fighters.Count == 1)
            {
                result.OpeningLine = "Only one of you managed to scrape together a monster?  No shows tonight, but rewards for the one busy beaver.";
                _battleService.GrantCash(fighters.Dequeue(), 1);
            }
            else
            {
                decimal countTotal = fighters.Count;
                //fight in rounds
                while (fighters.Count != 0)
                {
                    int round = 0;
                    if (fighters.Count == 1)
                    {
                        var winner = fighters.Dequeue();
                        result.ClosingLine = $"And we have a victor.\r\nCongratulations to {winner.Name} on {winner.Monster.Name}'s win!";
                        _battleService.GrantCash(winner, round);
                    }
                    else
                    {
                        PlayerData left = fighters.Dequeue();
                        PlayerData right = fighters.Dequeue();

                        var battle = _battleService.MonsterFight(left, right);

                        result.Fights.Add(battle);

                        fighters.Enqueue(battle.Winner);

                    }
                    if (fighters.Count <= Math.Ceiling(countTotal / 2))
                    {
                        round++;
                        countTotal = fighters.Count;
                    }

                }

            }

            //apply luck to losers
            return result;
        }
    }
}
