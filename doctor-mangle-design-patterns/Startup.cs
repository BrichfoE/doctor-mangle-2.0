using doctor_mangle;
using doctor_mangle.interfaces;
using doctor_mangle.Service;
using doctor_mangle.services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace doctor_mangle_design_patterns
{
    public static class Startup
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //var serviceCollection = new ServiceCollection()
            //    .AddSingleton<Program>();
            //ServiceRegistration.GetServiceCollection(serviceCollection);
            return Host.CreateDefaultBuilder(args)
                //.ConfigureServices(services => { services = serviceCollection; } );
                .ConfigureServices(services => { services
                    .AddTransient<IPlayerService, PlayerService>()
                    .AddTransient<IParkService, ParkService>()
                    .AddTransient<IBattleService, BattleService>()
                    .AddSingleton<Program>()
                    .AddSingleton<Random>()
                    .AddSingleton<GameController>();
                });
        }
    }
}
