using CryptoPaymentGateway.Infrastructure.Nginx.Commands;
using EngineFramework;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SchedulerFramework;

namespace CryptoPaymentGateway.Infrastructure.Nginx;

public class DependencyRegister : IDependencyRegister
{
    public void ServicesRegister(IServiceCollection services)
    {
        services.AddHostedService<NginxBackgroundService>();

        if (IDependencyRegister.Configuration?["EnableNginx"] == "true" && IDependencyRegister.Configuration["UsingLetsEncrypt"] == "true")
        {
            services.AddHostedService<CommandBackgroundService<SchedulerCommandBase>>();
        
            services.AddQuartz(q =>
                {
                    
                })
                // ASP.NET Core hosting
                .AddQuartzServer(options =>
                {
                    // when shutting down we want jobs to complete gracefully
                    options.WaitForJobsToComplete = true;
                });

            services.AddTransient<CommandTaskExecutor<SchedulerCommandBase>, ConcreteTaskExecutor>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var maxThreads = configuration["Scheduler:MaxThreads"] != null
                    ? Convert.ToInt32(configuration["Scheduler:MaxThreads"])
                    : 8;
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                return new ConcreteTaskExecutor(scopeFactory, configuration, serviceProvider.GetRequiredService<ILogger<ConcreteTaskExecutor>>(), maxThreads);
            });
            
            ((ServiceRegistry)services).Scan(scanner =>
            {
                scanner.AssembliesFromApplicationBaseDirectory();
                //Add all job registers for scheduler
                scanner.AddAllTypesOf<IJobRegister>(ServiceLifetime.Transient)
                    .NameBy(type => type.Name.ToLower());
            });
        }
    }
}