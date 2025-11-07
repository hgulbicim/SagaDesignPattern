using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Choreography;

namespace SagaChoreographyWorker.Consumers;

public class ShippingConsumer : IConsumer<IInventoryReserved>
{
    private readonly ILogger<ShippingConsumer> _logger;
    public ShippingConsumer(ILogger<ShippingConsumer> logger) => _logger = logger;

    public async Task Consume(ConsumeContext<IInventoryReserved> context)
    {
        _logger.LogInformation("Shipping order OrderId={OrderId}", context.Message.OrderId);

        try
        {
            var trackingNumber = $"TR-{Guid.NewGuid()}";

            await context.Publish<IOrderShipped>(new
            {
                context.Message.OrderId,
                TrackingNumber = trackingNumber,
                ShippedAt = DateTime.UtcNow
            });
        }
        catch
        {
            // Publish ShippingFailed Event 
        }
    }
}