using CliWrap;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoPaymentGateway.Infrastructure.Nginx;

public class NginxBackgroundService : BackgroundService
{
    private readonly IServer server;
    private readonly IHostApplicationLifetime hostApplicationLifetime;
    private readonly IConfiguration config;
    private readonly ILogger<NginxBackgroundService> logger;

    public NginxBackgroundService(
        IServer server,
        IHostApplicationLifetime hostApplicationLifetime,
        IConfiguration config,
        ILogger<NginxBackgroundService> logger
    )
    {
        this.server = server;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this.config = config;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!Convert.ToBoolean(config["EnableNginx"]) || !InDocker)
        {
            return;
        }
        
        await WaitForApplicationStarted();

        var task = StartTorTunnel(stoppingToken);

        // var publicUrl = await GetNgrokPublicUrl();
        // logger.LogInformation("Public ngrok URL: {NgrokPublicUrl}", publicUrl);
        logger.LogInformation("Starting nginx");

        try
        {
            await task;
        }
        finally
        {
            logger.LogInformation("nginx stopped");
        }
    }

    private Task WaitForApplicationStarted()
    {
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        hostApplicationLifetime.ApplicationStarted.Register(() => completionSource.TrySetResult());
        return completionSource.Task;
    }

    private CommandTask<CommandResult> StartTorTunnel(CancellationToken stoppingToken)
    {
        var torTask = Cli.Wrap("nginx")
            .WithArguments(args => args
                .Add("-g")
                .Add("daemon off;"))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => logger.LogDebug(s)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => logger.LogError(s)))
            .ExecuteAsync(stoppingToken);
        return torTask;
    }
    
    private bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";} }
}