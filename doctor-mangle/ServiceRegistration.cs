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
                .AddTransient<IArenaService, ArenaService>()
                .AddTransient<IBattleService, BattleService>()
                .AddSingleton<IGameService, GameService>()
                .AddTransient<IMonsterService, MonsterService>()
                .AddTransient<IParkService, ParkService>()
                .AddTransient<IPartService, PartService>()
                .AddTransient<IPlayerService, PlayerService>();
        }
    }
}
