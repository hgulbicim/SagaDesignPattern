using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Orchestration;

namespace SagaOrchestrationWorker;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State PaymentProcessing { get; private set; }
    public State InventoryReserving { get; private set; }
    public State Shipping { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<IOrderCreated> OrderCreated { get; private set; }

    public Request<OrderState, IProcessPayment, IPaymentAuthorized> ProcessPayment { get; private set; }
    public Request<OrderState, IReserveInventory, IInventoryReserved> ReserveInventory { get; private set; }
    public Request<OrderState, IShipOrderRequest, IOrderShipped> ShipOrderRequest { get; private set; }

    private readonly ILogger<OrderStateMachine> _logger;

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated, x => x.CorrelateById(m => m.Message.OrderId));

        Request(() => ProcessPayment, r => r.Timeout = TimeSpan.FromSeconds(30));
        Request(() => ReserveInventory, r => r.Timeout = TimeSpan.FromSeconds(15));
        Request(() => ShipOrderRequest, r => r.Timeout = TimeSpan.FromSeconds(60));

        ConfigureInitialState();
        ConfigurePaymentProcessingState();
        ConfigureInventoryReservingState();
        ConfigureShippingState();
        ConfigureCompletedState();
        ConfigureFailedState();

        SetCompletedWhenFinalized();
    }

    private void ConfigureInitialState()
    {
        Initially(
            When(OrderCreated)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.CustomerEmail = ctx.Message.CustomerEmail;
                    ctx.Saga.OrderTotal = ctx.Message.OrderTotal;
                    ctx.Saga.CreatedAt = ctx.Message.CreatedAt;
                    ctx.Saga.ShippingAddress = ctx.Message.ShippingAddress;
                    ctx.Saga.OrderItems = ctx.Message.Items;

                    _logger.LogInformation(
                        "Order started: OrderId={OrderId}, Total={Total}",
                        ctx.Saga.OrderId, ctx.Saga.OrderTotal);
                })
                .Request(ProcessPayment, ctx => ctx.Init<IProcessPayment>(new
                {
                    OrderId = ctx.Message.OrderId,
                    CustomerId = ctx.Message.CustomerId,
                    OrderTotal = ctx.Message.OrderTotal,
                    PaymentMethod = ctx.Message.PaymentMethod,
                    Timestamp = DateTime.UtcNow
                }))
                .TransitionTo(PaymentProcessing)
        );
    }

    private void ConfigurePaymentProcessingState()
    {
        During(PaymentProcessing,
            When(ProcessPayment.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.PaymentTransactionId = ctx.Message.TransactionId;
                    _logger.LogInformation(
                        "Payment approved: OrderId={OrderId}",
                        ctx.Saga.OrderId);
                })
                .Request(ReserveInventory, ctx => ctx.Init<IReserveInventory>(new
                {
                    OrderId = ctx.Message.OrderId,
                    Items = ctx.Saga.OrderItems,
                    Timestamp = DateTime.UtcNow
                }))
                .TransitionTo(InventoryReserving),

            When(ProcessPayment.Faulted)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Payment processing failed";
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    _logger.LogError("Payment failed: OrderId={OrderId}", ctx.Saga.OrderId);
                })
                .TransitionTo(Failed)
                .Finalize(),

            When(ProcessPayment.TimeoutExpired)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Payment timeout";
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    _logger.LogWarning("Payment timeout: OrderId={OrderId}", ctx.Saga.OrderId);
                })
                .TransitionTo(Failed)
                .Finalize()
        );
    }

    private void ConfigureInventoryReservingState()
    {
        During(InventoryReserving,
            When(ReserveInventory.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.ReservationId = ctx.Message.ReservationId;
                    _logger.LogInformation(
                        "Inventory reserved: OrderId={OrderId}",
                        ctx.Saga.OrderId);
                })
                .Request(ShipOrderRequest, ctx => ctx.Init<IShipOrderRequest>(new
                {
                    OrderId = ctx.Message.OrderId,
                    ShippingAddress = ctx.Saga.ShippingAddress,
                    Timestamp = DateTime.UtcNow
                }))
                .TransitionTo(Shipping),

            When(ReserveInventory.Faulted)
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.FailureReason = "Inventory not available";
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    _logger.LogWarning("Inventory failed: OrderId={OrderId}", ctx.Saga.OrderId);

                    await ctx.Publish<IRefundPayment>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        TransactionId = ctx.Saga.PaymentTransactionId,
                        RefundAmount = ctx.Saga.OrderTotal,
                        Timestamp = DateTime.UtcNow
                    });
                })
                .TransitionTo(Failed)
                .Finalize(),

            When(ReserveInventory.TimeoutExpired)
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.FailureReason = "Inventory timeout";
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    _logger.LogWarning("Inventory timeout: OrderId={OrderId}", ctx.Saga.OrderId);

                    await ctx.Publish<IRefundPayment>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        TransactionId = ctx.Saga.PaymentTransactionId,
                        RefundAmount = ctx.Saga.OrderTotal,
                        Timestamp = DateTime.UtcNow
                    });
                })
                .TransitionTo(Failed)
                .Finalize()
        );
    }

    private void ConfigureShippingState()
    {
        During(Shipping,
            When(ShipOrderRequest.Completed)
                .Then(ctx =>
                {
                    ctx.Saga.TrackingNumber = ctx.Message.TrackingNumber;
                    ctx.Saga.ShippedAt = ctx.Message.ShippedAt;
                    _logger.LogInformation(
                        "Order shipped: OrderId={OrderId}, Tracking={Tracking}",
                        ctx.Saga.OrderId, ctx.Saga.TrackingNumber);
                })
                .TransitionTo(Completed),

            When(ShipOrderRequest.Faulted)
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.FailureReason = "Shipping failed";
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    _logger.LogWarning("Shipping failed: OrderId={OrderId}", ctx.Saga.OrderId);

                    await ctx.Publish<IReleaseInventory>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        ReservationId = ctx.Saga.ReservationId,
                        Timestamp = DateTime.UtcNow
                    });

                    await ctx.Publish<IRefundPayment>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        TransactionId = ctx.Saga.PaymentTransactionId,
                        RefundAmount = ctx.Saga.OrderTotal,
                        Timestamp = DateTime.UtcNow
                    });
                })
                .TransitionTo(Failed)
                .Finalize(),

            When(ShipOrderRequest.TimeoutExpired)
                .ThenAsync(async ctx =>
                {
                    ctx.Saga.FailureReason = "Shipping timeout";
                    ctx.Saga.FailedAt = DateTime.UtcNow;
                    _logger.LogWarning("Shipping timeout: OrderId={OrderId}", ctx.Saga.OrderId);

                    await ctx.Publish<IReleaseInventory>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        ReservationId = ctx.Saga.ReservationId,
                        Timestamp = DateTime.UtcNow
                    });

                    await ctx.Publish<IRefundPayment>(new
                    {
                        OrderId = ctx.Saga.OrderId,
                        TransactionId = ctx.Saga.PaymentTransactionId,
                        RefundAmount = ctx.Saga.OrderTotal,
                        Timestamp = DateTime.UtcNow
                    });
                })
                .TransitionTo(Failed)
                .Finalize()
        );
    }

    private void ConfigureCompletedState()
    {
        During(Completed,
            Ignore(OrderCreated)
        );
    }

    private void ConfigureFailedState()
    {
        During(Failed,
            Ignore(OrderCreated)
        );
    }
}