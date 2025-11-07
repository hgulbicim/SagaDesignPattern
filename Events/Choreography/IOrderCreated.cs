using MassTransit;

namespace Shared.Events.Choreography;

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


public interface IPaymentAuthorized : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string TransactionId { get; }
    decimal Amount { get; }
    DateTime AuthorizedAt { get; }
}


public interface IInventoryReserved : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string ReservationId { get; }
    DateTime ReservedAt { get; }
}


public interface IOrderShipped : CorrelatedBy<Guid>
{
    Guid OrderId { get; }
    string TrackingNumber { get; }
    DateTime ShippedAt { get; }
}