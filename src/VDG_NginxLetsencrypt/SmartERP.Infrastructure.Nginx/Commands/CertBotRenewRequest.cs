using MediatR;

namespace CryptoPaymentGateway.Infrastructure.Nginx.Commands;

public class CertBotRenewRequest : SchedulerCommandBase, IRequest<string>
{
    
}