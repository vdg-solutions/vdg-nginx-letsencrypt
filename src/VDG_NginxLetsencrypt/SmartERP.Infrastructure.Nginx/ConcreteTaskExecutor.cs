using CryptoPaymentGateway.Infrastructure.Nginx.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiThreadsEngine;
using SchedulerFramework;

namespace CryptoPaymentGateway.Infrastructure.Nginx;

public class ConcreteTaskExecutor : CommandTaskExecutor<SchedulerCommandBase>
{
    private readonly ILogger<ConcreteTaskExecutor> _logger;

    public ConcreteTaskExecutor(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration,
        ILogger<ConcreteTaskExecutor> logger, int maxThreads = 8) : base(serviceScopeFactory, configuration, maxThreads)
    {
        _logger = logger;
    }
   
    protected override async Task ExecuteTask(SchedulerCommandBase job, int? taskNum, CancellationToken stoppingToken)
    {
        Console.WriteLine($"Task {taskNum} with job {job} executing at {DateTime.Now}");
    
        var doTheJob = new DoTheJob(_serviceScopeFactory, 5 * 60 * 1000, _logger); //Set a time slot for 5 mins
        await doTheJob.Run(job, stoppingToken);
    
        Console.WriteLine(
            $"Task {taskNum} DONE, if you do not see any info, means the task get errors");
    }

    private class DoTheJob : TaskWithTimeOutBase<SchedulerCommandBase>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ILogger<ConcreteTaskExecutor> _logger;
        // private readonly TradeManager _tradeManager;
    
        public DoTheJob(IServiceScopeFactory serviceScopeFactory, int timeSlot,
            ILogger<ConcreteTaskExecutor> logger) : base(timeSlot)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            // _tradeManager = new TradeManager();
        }
    
        protected override async Task ExecuteTask(SchedulerCommandBase schedulerCommandBase, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var services = scope.ServiceProvider;
            var mediator = services.GetRequiredService<IMediator>();
            Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            try
            {
                var ret = await mediator.Send(schedulerCommandBase, cancellationToken);
                // var ret = await _tradeManager.DoCommand(scope.ServiceProvider.GetRequiredService<IMediator>(), command, cancellationToken);
                _logger.LogInformation("CommandTaskExecutor: " + ret);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error when executing task {schedulerCommandBase.GetType()}, exception {e}");
            }
        }
    }
}