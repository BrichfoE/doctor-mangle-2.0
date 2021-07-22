using doctor_mangle.models;
using System;
using System.Collections.Generic;

namespace doctor_mangle.interfaces
{
    // contract that the individual game implementation should use and extend as needed
    public interface IGameRepository
    {
        void FileSetup();
        void SaveGame(GameData gd);
        bool CanLoadGames();
        GameData LoadGame(int gameId);
        int GetNextGameID();
        int GetGameIdFromName(string name);
        List<string> GetSavedGameNames();
        void LogException(GameData gd, string exceptionText, Exception ex, bool willClose);
    }
}
