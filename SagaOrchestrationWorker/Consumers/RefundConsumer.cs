using MassTransit;
using Shared.Events.Orchestration;

namespace SagaOrchestrationWorker.Consumers;

public class RefundConsumer : IConsumer<IRefundPayment>
{
    public async Task Consume(ConsumeContext<IRefundPayment> context)
    {
        var message = context.Message;

        await Task.Delay(200);

        await context.Publish<IPaymentRefunded>(new
        {
            OrderId = message.OrderId,
            RefundTransactionId = Guid.NewGuid().ToString(),
            RefundedAmount = message.RefundAmount,
            RefundedAt = DateTime.UtcNow
        });
    }
}
