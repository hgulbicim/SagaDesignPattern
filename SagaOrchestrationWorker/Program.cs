using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using SagaOrchestrationWorker;
using SagaOrchestrationWorker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentConsumer>();
    x.AddConsumer<InventoryConsumer>();
    x.AddConsumer<ShippingConsumer>();
    x.AddConsumer<RefundConsumer>();
    x.AddConsumer<InventoryReleaseConsumer>();

    x.AddQuartzConsumers();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .InMemoryRepository();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("host.docker.internal", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.UseMessageScheduler(new Uri("queue:quartz"));
    });
});


builder.Services.AddQuartz();

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddLogging();

var app = builder.Build();

await app.RunAsync();