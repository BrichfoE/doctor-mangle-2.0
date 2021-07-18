using doctor_mangle;
using doctor_mangle.interfaces;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace doctor_mangle_design_patterns
{
    public class Program
    {
        private readonly GameController _gc;
        private readonly IGameRepository _gr;

        public Program(GameController gc, IGameRepository gr) { _gc = gc; _gr = gr; }
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Services.GetRequiredService<Program>().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => {
                    services
                .AddTransient<IPlayerService, PlayerService>()
                .AddTransient<IParkService, ParkService>()
                .AddTransient<IBattleService, BattleService>()
                .AddScoped<IGameService, GameService>()
                .AddTransient<IComparer<BodyPart>, PartComparer>()
                .AddSingleton<IGameRepository, GameRepo>()
                .AddSingleton<Program>()
                .AddSingleton<Random>()
                .AddSingleton<GameController>();
                });
        }

        public void Run()
        {
            bool activeGame = true;

            while (activeGame)
            {
                try
                {
                    activeGame = _gc.RunGame();
                }
                catch (System.Exception ex)
                {
                    string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                    int currentLine = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileLineNumber();
                    _gr.LogException(_gc.Data, $"General exception {currentFile} line {currentLine}", ex, true);
                    activeGame = false;
                }
            }
        }
    }
}
