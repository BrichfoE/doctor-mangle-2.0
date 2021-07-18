using doctor_mangle.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace doctor_mangle.interfaces
{
    public interface IGameService
    {
        GameData GetNewGameData(string name, int aiCount, int gameID);
        string PrintRegionOptions(GameData gameData);
        void AISearchTurn(GameData gd, int round);
        void AIBuildTurn(GameData data);
    }
}
