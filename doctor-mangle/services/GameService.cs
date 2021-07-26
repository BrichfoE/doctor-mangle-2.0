using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models;
using doctor_mangle.models.monsters;
using doctor_mangle.models.parts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace doctor_mangle.services
{
    public class GameService : IGameService
    {
        private readonly ILogger<IGameService> _logger;
        private readonly IParkService _parkService;
        private readonly IPlayerService _playerService;
        private readonly IComparer<BodyPart> _partComparer;
        private readonly Random _rng;

        public GameService(
            ILogger<IGameService> logger,
            IPlayerService playerService,
            IParkService parkService,
            IComparer<BodyPart> partComparer,
            Random random)
        {
            this._logger = logger;
            this._parkService = parkService;
            this._playerService = playerService;
            this._partComparer = partComparer;
            this._rng = random;
        }

        public GameData GetNewGameData(string name, int aiCount, int gameID)
        {
            _logger.LogInformation($"GetNewGameData: Generating new GameDataObject '{name}' with gameId: {gameID}.");
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
            int min = 1, max = 4;
            _logger.LogInformation($"PrintRegionOptions: Printing regions to which a player may move, between {min} and {max}.");
            string result = string.Empty;
            for (int i = min; i < max+1; i++)
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
        // TODO: move this player logic to the player service, maybe leave the iteration here to manage orchestration
        public void AIBuildTurn(GameData data)
        {
            _logger.LogInformation($"AIBuildTurn: Orchestrating build for AI players, day: {data.GameDayNumber}.");
            foreach (var ai in data.AiPlayers)
            {
                _playerService.TryBuildMonster(ai);
            }
        }

        public void AISearchTurn(GameData gd, int round)
        {
            _logger.LogInformation($"AISearchTurn: Orchestrating search for AI players, day: {gd.GameDayNumber}, round; {round}.");
            foreach (var ai in gd.AiPlayers)
            {
                int region = _rng.Next(1, 4);
                if (_parkService.SearchForPart(gd.Parks[region], out BodyPart part))
                {
                    ai.Bag[round - 1] = part;
                }
            }
        }

        public void AdvancePhase(GameData data)
        {
            data.GamePhase = data.GamePhase == Phase.Night
                ? Phase.Search
                : data.GamePhase + 1;
        }
    }
}
