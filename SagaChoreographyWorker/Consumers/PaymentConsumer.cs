using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Choreography;

namespace SagaChoreographyWorker.Consumers;

public class PaymentConsumer : IConsumer<IOrderCreated>
{
    private readonly ILogger<PaymentConsumer> _logger;
    public PaymentConsumer(ILogger<PaymentConsumer> logger) => _logger = logger;

    public async Task Consume(ConsumeContext<IOrderCreated> context)
    {
        _logger.LogInformation("Processing payment for OrderId={OrderId}", context.Message.OrderId);

        try
        {
            var transactionId = Guid.NewGuid();

            await context.Publish<IPaymentAuthorized>(new
            {
                context.Message.OrderId,
                TransactionId = transactionId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch
        {
            // Publish PaymentFailed Event
        }
    }
}
