using doctor_mangle;
using doctor_mangle_data.controllers;
using System;

namespace doctor_mangle_design_patterns
{
    class Program
    {
        static void Main(string[] args)
        {
            GameController gc = new GameController();
            bool activeGame = true;

            while (activeGame)
            {
                try
                {
                    activeGame = gc.RunGame();
                }
                catch (System.Exception ex)
                {
                    string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                    int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                    gc.Repo.LogException(gc.Data, $"General exception {currentFile} line {currentLine}", ex, true);
                    activeGame = false;
                }
            }
        }
    }
}
