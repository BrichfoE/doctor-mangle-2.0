using doctor_mangle.interfaces;
using doctor_mangle.services;
using Microsoft.Extensions.DependencyInjection;

namespace doctor_mangle
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IGameService, GameService>()
                .AddTransient<IPlayerService, PlayerService>()
                .AddTransient<IParkService, ParkService>()
                .AddTransient<IBattleService, BattleService>()
                .AddTransient<IMonsterService, MonsterService>();
        }
    }
}
