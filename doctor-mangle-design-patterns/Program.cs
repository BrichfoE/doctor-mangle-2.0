using doctor_mangle;
using Microsoft.Extensions.DependencyInjection;

namespace doctor_mangle_design_patterns
{
    public class Program
    {
        private readonly GameController _gc;

        public Program(GameController gc) { _gc = gc; }
        static void Main(string[] args)
        {
            var host = Startup.CreateHostBuilder(args).Build();
            host.Services.GetRequiredService<Program>().Run();
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
                    _gc.Repo.LogException(_gc.Data, $"General exception {currentFile} line {currentLine}", ex, true);
                    activeGame = false;
                }
            }
        }
    }
}
