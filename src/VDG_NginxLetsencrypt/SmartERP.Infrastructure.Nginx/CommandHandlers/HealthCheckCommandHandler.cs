using CryptoPaymentGateway.Infrastructure.Nginx.Commands;
using MediatR;

namespace CryptoPaymentGateway.Infrastructure.Nginx.CommandHandlers;

public class HealthCheckCommandHandler : IRequestHandler<HealthCheckRequest, string>
{
    public async Task<string> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
    {
        return $"HealthCheck at {DateTime.Now}: OK";
    }
}