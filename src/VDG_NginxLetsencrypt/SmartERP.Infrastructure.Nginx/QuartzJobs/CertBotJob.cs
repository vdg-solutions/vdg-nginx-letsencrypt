using CryptoPaymentGateway.Infrastructure.Nginx.Commands;
using Quartz;
using SchedulerFramework;

namespace CryptoPaymentGateway.Infrastructure.Nginx.QuartzJobs;

public class CertBotJob : IJob, IJobRegister
{
    public Task Execute(IJobExecutionContext context)
    {
        CommandBackgroundService<SchedulerCommandBase>.RegisterWrapper.RegisterToRun(new CertBotRenewRequest());
        return Task.CompletedTask;
    }

    public async Task RegisterJob(IScheduler scheduler, CancellationToken stoppingToken)
    {
        var quartsJob = JobBuilder.Create<CertBotJob>()
            .WithIdentity("certBotJob", "group2") // name "healthCheckJob", group "group1"
            .Build();
        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithIdentity("certBotJobTrigger", "group2")
            .StartNow()
            .WithCronSchedule("0 12 * * *")
            // .WithSimpleSchedule(builder => builder
            //     .WithIntervalInSeconds(20)
            //     .RepeatForever()
            // )
            .Build();
        // Tell quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(quartsJob, trigger, stoppingToken);
    }
}