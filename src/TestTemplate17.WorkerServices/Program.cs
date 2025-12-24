using System;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SparkRoseDigital.Infrastructure.Caching;
using TestTemplate17.Common.MessageBroker.Middlewares.ErrorLogging;
using TestTemplate17.Common.MessageBroker.Middlewares.Tracing;
using TestTemplate17.Core;
using TestTemplate17.Core.Events;
using TestTemplate17.Data;
using TestTemplate17.WorkerServices.FaultService;
using TestTemplate17.WorkerServices.FooService;
using LoggerExtensions = SparkRoseDigital.Infrastructure.Logging.LoggerExtensions;

namespace TestTemplate17.WorkerServices;

public class Program
{
    public static void Main(string[] args)
    {
        LoggerExtensions.ConfigureSerilogLogger("DOTNET_ENVIRONMENT");

        try
        {
            Log.Information("Starting up TestTemplate17 Worker Services.");
            CreateHostBuilder(args)
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "TestTemplate17 Worker Services failed at startup.");
        }
        finally
        {
            Log.CloseAndFlush();
        }

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var hostEnvironment = hostContext.HostingEnvironment;
                services.AddDbContext<TestTemplate17DbContext>(options =>
                {
                    var connString = new SqlConnectionStringBuilder(configuration["TestTemplate17DbConnection"]);
                    if (hostEnvironment.IsDevelopment())
                    {
                        connString.UserID = configuration["DbUser"] ?? string.Empty;
                        connString.Password = configuration["DbPassword"] ?? string.Empty;
                    }
                    else
                    {
                        connString.Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity;
                    }
                    options.UseSqlServer(connString.ConnectionString);
                    if (hostEnvironment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging(true);
                    }
                });
                services.AddGenericRepository();
                services.AddSpecificRepositories();
                services.AddCoreServices();
                services.AddSingleton<ICache, Cache>();
                services.AddMemoryCache();
                services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(Program).Assembly);

                services.AddMassTransit(x =>
                {
                    x.AddConsumer<FooConsumer>();
                    x.AddConsumer<FooFaultConsumer>();
                    x.AddConsumer<FaultConsumer>();
                    x.AddConsumer<FooCommandConsumer>(typeof(FooCommandConsumer.FooCommandConsumerDefinition));

                    if (string.IsNullOrEmpty(configuration["MessageBroker"]))
                    {
                        x.UsingInMemory();
                    }
                    else
                    {
                        x.UsingAzureServiceBus((ctx, cfg) =>
                        {
                            cfg.Host(configuration["MessageBroker"]);

                            // Use the below line if you are not going with
                            // SetKebabCaseEndpointNameFormatter() in the publishing project (see API project),
                            // but have rather given the topic a custom name.
                            // cfg.Message<VoteCast>(configTopology => configTopology.SetEntityName("foo-topic"));
                            cfg.SubscriptionEndpoint<IFooEvent>("foo-event-subscription-1", e =>
                            {
                                e.ConfigureConsumer<FooConsumer>(ctx);
                            });

                            // This is here only for show. I have not thought through a proper
                            // error handling strategy.
                            cfg.SubscriptionEndpoint<Fault<IFooEvent>>("foo-event-fault-consumer", e =>
                            {
                                e.ConfigureConsumer<FooFaultConsumer>(ctx);
                            });

                            // This is here only for show. I have not thought through a proper
                            // error handling strategy.
                            cfg.SubscriptionEndpoint<Fault>("fault-consumer", e =>
                            {
                                e.ConfigureConsumer<FaultConsumer>(ctx);
                            });
                            cfg.ConfigureEndpoints(ctx);

                            cfg.UseMessageBrokerTracing();
                            cfg.UseExceptionLogger(services);
                        });
                    }
                    x.AddEntityFrameworkOutbox<TestTemplate17DbContext>(o =>
                    {
                        // configure which database lock provider to use (Postgres, SqlServer, or MySql)
                        o.UseSqlServer();

                        // enable the bus outbox
                        o.UseBusOutbox();
                    });
                });
                if (!string.IsNullOrEmpty(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
                {
                    services
                        .AddOpenTelemetry()
                        .WithTracing(tracerProviderBuilder =>
                        {
                            tracerProviderBuilder
                                .AddSource(WorkerAssemblyInfo.Value.GetName().Name)
                                .SetResourceBuilder(
                                    ResourceBuilder
                                        .CreateDefault()
                                        .AddService(serviceName: WorkerAssemblyInfo.Value.GetName().Name))
                                .AddEntityFrameworkCoreInstrumentation()
                                .AddSqlClientInstrumentation()
                                .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
                                .AddAzureMonitorTraceExporter(o =>
                                {
                                    o.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                                });
                        })
                        .WithMetrics(meterProviderBuilder =>
                        {
                            // Resource describing which Meters report on which metrics:
                            // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/built-in-metrics
                            // Reason you might want to refer to this resource is so you know what metrics to
                            // look into when using Application Insights Metrics tab.
                            meterProviderBuilder
                                .SetResourceBuilder(
                                    ResourceBuilder
                                        .CreateDefault()
                                        .AddService(serviceName: WorkerAssemblyInfo.Value.GetName().Name))
                                .AddRuntimeInstrumentation()
                                .AddMeter(InstrumentationOptions.MeterName) // MassTransit Meter: https://masstransit.io/documentation/configuration/observability
                                .AddAzureMonitorMetricExporter(o =>
                                {
                                    o.ConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                                });
                        });
                }
            });
}
