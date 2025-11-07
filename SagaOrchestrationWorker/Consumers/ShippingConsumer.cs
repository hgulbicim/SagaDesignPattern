using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Orchestration;

namespace SagaOrchestrationWorker.Consumers;

public class ShippingConsumer : IConsumer<IShipOrderRequest>
{
    private readonly ILogger<ShippingConsumer> _logger;

    public ShippingConsumer(ILogger<ShippingConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IShipOrderRequest> context)
    {
        _logger.LogInformation(
            "Shipping: OrderId={OrderId}",
            context.Message.OrderId);

        await Task.Delay(2000);

        if (new Random().Next(0, 100) < 20)
        {
            _logger.LogWarning(
                "Shipment FAILED: OrderId={OrderId} - Shipment not available",
                context.Message.OrderId);

            throw new InvalidOperationException("Shipment not available for requested city");
        }

        var trackingNumber = $"TRACK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        await context.RespondAsync<IOrderShipped>(new
        {
            OrderId = context.Message.OrderId,
            TrackingNumber = trackingNumber,
            ShippedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Shipped: OrderId={OrderId}, Tracking={Tracking}",
            context.Message.OrderId, trackingNumber);
    }
}