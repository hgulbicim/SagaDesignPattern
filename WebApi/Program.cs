using MassTransit;
using Shared.Events.Orchestration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
{
    cfg.UsingRabbitMq((context, busCfg) =>
    {
        busCfg.Host("host.docker.internal", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        busCfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapPost("/orders/orchestration", async (IPublishEndpoint publish) =>
{
    var orderId = Guid.NewGuid();

    await publish.Publish<IOrderCreated>(new
    {
        OrderId = orderId,
        CorrelationId = orderId,
        CustomerId = Guid.NewGuid(),
        CustomerEmail = "customer@mail.com",
        OrderTotal = 99.99m,
        PaymentMethod = "CreditCard",
        Items = new[]
        {
            new OrderItem { ProductId = "SHOE-001", Quantity = 2, UnitPrice = 50m }
        },
        ShippingAddress = new Address
        {
            StreetAddress = "Elalem Ne Der Sokak",
            City = "Istanbul",
            PostalCode = "34704",
            Country = "Türkiye"
        },
        CreatedAt = DateTime.UtcNow
    });

    return Results.Accepted($"/orders/{orderId}", new { OrderId = orderId });
});


app.MapPost("/orders/choreography", async (IPublishEndpoint publish) =>
{
    var orderId = Guid.NewGuid();

    await publish.Publish<Shared.Events.Choreography.IOrderCreated>(new
    {
        OrderId = orderId,
        CorrelationId = orderId,
        CustomerId = Guid.NewGuid(),
        CustomerEmail = "customer@mail.com",
        OrderTotal = 99.99m,
        PaymentMethod = "CreditCard",
        Items = new[]
        {
            new Shared.Events.Choreography.OrderItem { ProductId = "SHOE-001", Quantity = 2, UnitPrice = 50m }
        },
        ShippingAddress = new Shared.Events.Choreography.Address
        {
            StreetAddress = "Elalem Ne Der Sokak",
            City = "Istanbul",
            PostalCode = "34704",
            Country = "Türkiye"
        },
        CreatedAt = DateTime.UtcNow
    });

    return Results.Accepted($"/orders/{orderId}", new { OrderId = orderId });
});

app.Run();