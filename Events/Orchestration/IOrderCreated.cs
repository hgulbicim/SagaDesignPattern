using MassTransit;

namespace Shared.Events.Orchestration;

public interface IOrderCreated : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    Guid CustomerId { get; }
    string CustomerEmail { get; }
    decimal OrderTotal { get; }
    string PaymentMethod { get; }
    OrderItem[] Items { get; }
    Address ShippingAddress { get; }
    DateTime CreatedAt { get; }
}

public class OrderItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Address
{
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}

public interface IProcessPayment : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    Guid CustomerId { get; }
    decimal OrderTotal { get; }
    string PaymentMethod { get; }
    DateTime Timestamp { get; }
}

public interface IPaymentAuthorized : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string TransactionId { get; }
    decimal Amount { get; }
    DateTime AuthorizedAt { get; }
}

public interface IReserveInventory : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    OrderItem[] Items { get; }
    DateTime Timestamp { get; }
}

public interface IInventoryReserved : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string ReservationId { get; }
    DateTime ReservedAt { get; }
}

public interface IReleaseInventory : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string ReservationId { get; }
    DateTime Timestamp { get; }
}

public interface IShipOrderRequest : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    Address ShippingAddress { get; }
    DateTime Timestamp { get; }
}

public interface IOrderShipped : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string TrackingNumber { get; }
    DateTime ShippedAt { get; }
}

public interface IRefundPayment : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string TransactionId { get; }
    decimal RefundAmount { get; }
    DateTime Timestamp { get; }
}

public interface IPaymentRefunded : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string RefundTransactionId { get; }
    decimal RefundedAmount { get; }
    DateTime RefundedAt { get; }
}

public interface IOrderCompleted : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    Guid CustomerId { get; }
    DateTime CompletedAt { get; }
}

public interface IOrderCompensationStarted : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    Guid CustomerId { get; }
    string FailureReason { get; }
    DateTime Timestamp { get; }
}