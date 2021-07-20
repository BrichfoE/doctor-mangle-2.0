using doctor_mangle;
using doctor_mangle.interfaces;
using doctor_mangle.models.parts;
using doctor_mangle.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace doctor_mangle_console_app
{
    public class Program
    {
        private readonly GameController _gc;
        private readonly GameRepo _gr;

        public Program(GameController gc, GameRepo gr) { _gc = gc; _gr = gr; }
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Services.GetRequiredService<Program>().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => {
                    _ = services.AddScoped<IGameService, GameService>()
                        .AddTransient<IPlayerService, PlayerService>()
                        .AddTransient<IParkService, ParkService>()
                        .AddTransient<IBattleService, BattleService>()
                        .AddTransient<IMonsterService, MonsterService>()
                        .AddTransient<IMonsterService, MonsterService>()
                        .AddTransient<IComparer<BodyPart>, PartComparer>()
                        .AddSingleton<GameRepo>()
                        .AddSingleton<Program>()
                        .AddSingleton<Random>()
                        .AddSingleton<GameController>();
                });
        }

        public void Run()
        {
            _gc.Init();
            bool activeGame = true;
            while (activeGame)
            {
                try
                {
                    activeGame = _gc.RunGame();
                }
                catch (Exception ex)
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
