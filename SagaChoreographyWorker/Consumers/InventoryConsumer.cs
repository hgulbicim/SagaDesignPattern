using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Choreography;

namespace SagaChoreographyWorker.Consumers;
public class InventoryConsumer : IConsumer<IPaymentAuthorized>
{
    private readonly ILogger<InventoryConsumer> _logger;
    public InventoryConsumer(ILogger<InventoryConsumer> logger) => _logger = logger;

    public async Task Consume(ConsumeContext<IPaymentAuthorized> context)
    {
        _logger.LogInformation("Reserving inventory for OrderId={OrderId}", context.Message.OrderId);

        try
        {
            var reservationId = Guid.NewGuid();

            await context.Publish<IInventoryReserved>(new
            {
                context.Message.OrderId,
                ReservationId = reservationId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch
        {
            // Publish InventoryReservationFailed Event
        }
    }
}