using doctor_mangle.interfaces;
using doctor_mangle.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doctor_mangle.data
{
    public abstract class GameRepositoryBase : IGameRepository
    {
        protected Dictionary<string, int> gameIndex = new Dictionary<string, int>();
        protected readonly IParkService _parkService;
        protected readonly IMonsterService _monsterService;
        protected readonly IPlayerService _playerService;

        public GameRepositoryBase(IParkService parkService, IMonsterService monsterService, IPlayerService playerService)
        {
            this._parkService = parkService;
            this._monsterService = monsterService;
            this._playerService = playerService;
        }

        public bool CanLoadGames()
        {
            // check if there's a placeholder in the index that would skew results
            var expected = gameIndex["_placeholder"] == 0 ? 1 : 0;
            return gameIndex.Count > expected;
        }

        public abstract void FileSetup();

        public int GetGameIdFromName(string name)
        {
            return gameIndex.ContainsKey(name)
                ? gameIndex[name]
                : -1;
        }

        public int GetNextGameID()
        {
            int GameID;

            if (gameIndex == null)
            {
                GameID = 1;
            }
            else
            {
                GameID = gameIndex.Last().Value + 1;
            }

            return GameID;
        }

        public List<string> GetSavedGameNames()
        {
            return gameIndex.Keys.ToList();
        }

        public GameData LoadGame(int gameId)
        {
            var data = LoadImplementation(gameId);
            MovePartsAfterDeserialization(data);
            return data;
        }

        protected abstract GameData LoadImplementation(int gameId);

        public abstract void LogException(GameData gd, string exceptionText, Exception ex, bool willClose);

        public void SaveGame(GameData gd)
        {
            MovePartsForSerilaization(gd);
            SaveImplementation(gd);
            MovePartsAfterDeserialization(gd);
        }

        protected abstract void SaveImplementation(GameData gd);

        protected void MovePartsForSerilaization(GameData gd)
        {
            foreach (var park in gd.Parks)
            {
                _parkService.MovePartsForSerilaization(park);
            }

            _playerService.MovePartsForSerilaization(gd.CurrentPlayer);
            if (gd.CurrentPlayer.Monster != null)
            {
                _monsterService.MovePartsForSerilaization(gd.CurrentPlayer.Monster);
            }
            foreach (var player in gd.AiPlayers)
            {
                _playerService.MovePartsForSerilaization(player);
                if (player.Monster != null)
                {
                    _monsterService.MovePartsForSerilaization(player.Monster);
                }
            }
        }

        protected void MovePartsAfterDeserialization(GameData gd)
        {
            foreach (var park in gd.Parks)
            {
                _parkService.MovePartsAfterDeserilaization(park);
            }
            if (gd.CurrentPlayer.Monster != null)
            {
                _playerService.MovePartsAfterDeserilaization(gd.CurrentPlayer);
                _monsterService.MovePartsAfterDeserilaization(gd.CurrentPlayer.Monster);
            }
            foreach (var player in gd.AiPlayers)
            {
                _playerService.MovePartsAfterDeserilaization(player);
                if (player.Monster != null)
                {
                    _monsterService.MovePartsAfterDeserilaization(player.Monster);
                }
            }
        }
    }
}
