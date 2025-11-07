using Microsoft.Extensions.Hosting;
using MassTransit;
using SagaChoreographyWorker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InventoryConsumer>();
    x.AddConsumer<PaymentConsumer>();
    x.AddConsumer<ShippingConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("host.docker.internal", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    });
});

var app = builder.Build();

await app.RunAsync();