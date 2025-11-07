#  🌀 Saga Design Pattern (**Orchestration** vs **Choreography**)

Distributed transaction management using **Orchestration** and **Choreography** patterns with MassTransit, RabbitMQ, and Quartz.NET in .NET 9.

## Overview

This project demonstrates two approaches to implementing the Saga pattern for managing distributed transactions:

- **Orchestration**: Central state machine (`OrderStateMachine`) coordinates all steps
- **Choreography**: Services react to events independently without central coordination

Both handle an e-commerce order flow: **Payment → Inventory → Shipping** with full compensation on failures.

## Orchestration vs Choreography

| Aspect | Orchestration | Choreography |
|--------|---------------|--------------|
| **Control** | Centralized state machine | Decentralized events |
| **Coordination** | Orchestrator manages flow | Services react to events |
| **Compensation** | Explicit by orchestrator | Implicit by services |
| **Complexity** | Easier to understand | Harder to trace |
| **Timeout** | Managed by Quartz.NET | Per-service handling |
| **Best For** | Complex workflows | Simple, autonomous services |

## Technologies

- **.NET 9** - Application framework
- **MassTransit 8** - Saga state machine & messaging
- **RabbitMQ** - Message broker
- **Quartz.NET** - Timeout management (orchestration)
- **Docker** - Containerization

## State Machine (Orchestration)

**Flow:**
```
Initial → PaymentProcessing → InventoryReserving → Shipping → Completed
             ↓                      ↓                  ↓
           Failed ←──────────────  Failed  ←────────  Failed
```

**States:** Initial, PaymentProcessing, InventoryReserving, Shipping, Completed, Failed

**Timeouts:** Payment (30s), Inventory (15s), Shipping (60s)

**Compensation:** On failure, orchestrator triggers refunds and inventory releases

## Key Features

### Retry Mechanism
```csharp
cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
```
- 3 retry attempts
- 5-second intervals
- Handles transient failures

### Timeout Management (Orchestration)
```csharp
cfg.UseMessageScheduler(new Uri("queue:quartz"));
```
Quartz.NET manages timeouts and triggers compensation on expiry.

## Getting Started

### Quick Start with Docker

```bash
# 1. Start RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# 2. Run services
docker-compose up --build
```

## API Endpoints

**Create Order - Orchestration:**
```bash
POST http://localhost:5000/orders/orchestration
```

**Create Order - Choreography:**
```bash
POST http://localhost:5000/orders/choreography
```

**Response:**
```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

## Testing Failures

The implementation includes simulated failures:
- Payment: 10% failure rate
- Inventory: 20% failure rate
- Shipping: 5% failure rate

**Monitor:**
- RabbitMQ Management UI: `http://localhost:15672` (guest/guest)
- Application logs for compensation events
- Retry attempts in action

**Example Flow:**
```
✅ Success: Order → Payment → Inventory → Shipping → Completed
❌ Failure: Order → Payment → Inventory FAILED → Refund → Failed
⏱️ Timeout: Order → Payment timeout (30s) → Failed
```

---

⭐ Star this repo if you find it useful!
