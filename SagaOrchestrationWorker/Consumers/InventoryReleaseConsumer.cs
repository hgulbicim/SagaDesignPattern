using MassTransit;
using Shared.Events.Orchestration;

namespace SagaOrchestrationWorker.Consumers;

public class InventoryReleaseConsumer : IConsumer<IReleaseInventory>
{
    public async Task Consume(ConsumeContext<IReleaseInventory> context)
    {
        await Task.Delay(300); 

        Console.WriteLine($"Inventory released for order {context.Message.OrderId}");
    }
}
