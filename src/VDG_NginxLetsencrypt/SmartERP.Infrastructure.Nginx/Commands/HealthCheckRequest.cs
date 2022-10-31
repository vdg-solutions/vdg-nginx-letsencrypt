using MediatR;

namespace CryptoPaymentGateway.Infrastructure.Nginx.Commands;

public class HealthCheckRequest : SchedulerCommandBase, IRequest<string>
{
}