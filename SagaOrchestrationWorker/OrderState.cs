using MassTransit;
using Shared.Events.Orchestration;

namespace SagaOrchestrationWorker;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public decimal OrderTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderItem[] OrderItems { get; set; }
    public string PaymentTransactionId { get; set; }
    public string ReservationId { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime? ShippedAt { get; set; }
    public Address ShippingAddress { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string FailureReason { get; set; }
    public DateTime UpdatedAt { get; set; }
}