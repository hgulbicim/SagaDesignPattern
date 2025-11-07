using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Orchestration;

public class InventoryConsumer : IConsumer<IReserveInventory>
{
    private readonly ILogger<InventoryConsumer> _logger;

    public InventoryConsumer(ILogger<InventoryConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IReserveInventory> context)
    {
        _logger.LogInformation(
            "Reserving inventory: OrderId={OrderId}, Items={ItemCount}",
            context.Message.OrderId, context.Message.Items.Length);

        await Task.Delay(500);

        if (new Random().Next(0, 100) < 20)
        {
            _logger.LogWarning(
                "Inventory FAILED: OrderId={OrderId} - Stock not available",
                context.Message.OrderId);

            throw new InvalidOperationException("Stock not available for requested items");
        }

        var reservationId = Guid.NewGuid().ToString();

        await context.RespondAsync<IInventoryReserved>(new
        {
            OrderId = context.Message.OrderId,
            ReservationId = reservationId,
            ReservedAt = DateTime.UtcNow
        });

        _logger.LogInformation(
            "Inventory reserved: OrderId={OrderId}, ReservationId={ReservationId}",
            context.Message.OrderId, reservationId);
    }
}