using CliWrap;
using CryptoPaymentGateway.Infrastructure.Nginx.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoPaymentGateway.Infrastructure.Nginx.CommandHandlers;

public class CertBorRenewCommandHandler : IRequestHandler<CertBotRenewRequest, string>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CertBorRenewCommandHandler> _logger;

    public CertBorRenewCommandHandler(IConfiguration configuration, ILogger<CertBorRenewCommandHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> Handle(CertBotRenewRequest request, CancellationToken cancellationToken)
    {
        if (!File.Exists("CertBot1stRun"))
        {
            // certbot --nginx -d example.com -d www.example.com
            await Cli.Wrap("/usr/bin/certbot")
                .WithArguments(args => args
                    .Add("--nginx")
                    .Add("-d")
                    .Add(_configuration["Domain"])
                )
                .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogDebug(s)))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)))
                .ExecuteAsync(cancellationToken);
            await File.WriteAllTextAsync("CertBot1stRun", string.Empty, cancellationToken);
            return "Done run 1st time";
        }
        
        // /usr/bin/certbot renew --quiet
        await Cli.Wrap("/usr/bin/certbot")
            .WithArguments(args => args
                .Add("renew")
                .Add("--quiet"))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogDebug(s)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)))
            .ExecuteAsync(cancellationToken);
        return "Done Renew";
    }
}