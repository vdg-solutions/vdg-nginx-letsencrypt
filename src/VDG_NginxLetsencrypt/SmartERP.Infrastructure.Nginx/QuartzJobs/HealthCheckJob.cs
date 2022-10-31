using CryptoPaymentGateway.Infrastructure.Nginx.Commands;
using Quartz;
using SchedulerFramework;

namespace CryptoPaymentGateway.Infrastructure.Nginx.QuartzJobs;

public class HealthCheckJob : IJob, IJobRegister
{
    public Task Execute(IJobExecutionContext context)
    {
        CommandBackgroundService<SchedulerCommandBase>.RegisterWrapper.RegisterToRun(new HealthCheckRequest());
        return Task.CompletedTask;
    }

    public async Task RegisterJob(IScheduler scheduler, CancellationToken stoppingToken)
    {
        var quartsJob = JobBuilder.Create<HealthCheckJob>()
            .WithIdentity("healthCheckJob", "group1") // name "healthCheckJob", group "group1"
            .Build();
        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithIdentity("healthCheckTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(builder => builder
                .WithIntervalInSeconds(20)
                .RepeatForever()
            )
            .Build();
        // Tell quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(quartsJob, trigger, stoppingToken);
    }
}