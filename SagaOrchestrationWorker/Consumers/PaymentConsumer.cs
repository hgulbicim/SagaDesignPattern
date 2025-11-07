using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Orchestration;

namespace SagaOrchestrationWorker.Consumers;

public class PaymentConsumer : IConsumer<IProcessPayment>
{
    private readonly ILogger<PaymentConsumer> _logger;

    public PaymentConsumer(ILogger<PaymentConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IProcessPayment> context)
    {
        _logger.LogInformation(
            "Processing payment: OrderId={OrderId}, Amount={Amount}",
            context.Message.OrderId, context.Message.OrderTotal);

        await Task.Delay(1000);
        
        if (new Random().Next(0, 100) < 20)
        {
            _logger.LogWarning(
                "Payment FAILED: OrderId={OrderId} - Account not available",
                context.Message.OrderId);

            throw new InvalidOperationException("Account not available for requested amount");
        }

        var transactionId = Guid.NewGuid().ToString();

        await context.RespondAsync<IPaymentAuthorized>(new
        {
            OrderId = context.Message.OrderId,
            TransactionId = transactionId,
            Amount = context.Message.OrderTotal,
            AuthorizedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Payment authorized: OrderId={OrderId}, TransactionId={TransactionId}",
            context.Message.OrderId, transactionId);
    }
}