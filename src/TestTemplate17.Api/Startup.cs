using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SparkRoseDigital.Infrastructure.Caching;
using SparkRoseDigital.Infrastructure.HealthCheck;
using SparkRoseDigital.Infrastructure.Logging;
using TestTemplate17.Api.Helpers;
using TestTemplate17.Api.Middlewares;
using TestTemplate17.Application;
using TestTemplate17.Core;
using TestTemplate17.Data;

namespace TestTemplate17.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;

    public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;
            configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest));
            configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status404NotFound));
            configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(object), StatusCodes.Status406NotAcceptable));
            configure.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
        });
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Startup>();

        services.AddDbContext<TestTemplate17DbContext>(options =>
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_configuration["TestTemplate17DbConnection"] ?? string.Empty);
            if (_hostEnvironment.IsDevelopment())
            {
                sqlConnectionStringBuilder.UserID = _configuration["DbUser"] ?? string.Empty;
                sqlConnectionStringBuilder.Password = _configuration["DbPassword"] ?? string.Empty;
            }
            else
            {
                sqlConnectionStringBuilder.Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity;
            }
            options.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
            if (_hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging(true);
            }
        });

        services.AddGenericRepository();
        services.AddSpecificRepositories();
        services.AddCoreServices();

        services
            .AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.Authority = _configuration["AuthAuthority"];
                options.Audience = _configuration["AuthAudience"];
                options.TokenValidationParameters.ValidIssuer = _configuration["AuthValidIssuer"];
            });

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddLoggingScopes();
        if (!string.IsNullOrEmpty(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            services
                .AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    if (_hostEnvironment.IsDevelopment())
                    {
                        tracerProviderBuilder.SetSampler<AlwaysOnSampler>();
                    }
                    tracerProviderBuilder
                        .AddSource(ApiAssemblyInfo.Value.GetName().Name)
                        .SetResourceBuilder(
                            ResourceBuilder
                                .CreateDefault()
                                .AddService(serviceName: ApiAssemblyInfo.Value.GetName().Name))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
                        .AddAzureMonitorTraceExporter(o =>
                        {
                            o.ConnectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
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
                                .AddService(serviceName: ApiAssemblyInfo.Value.GetName().Name))
                        .AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter(InstrumentationOptions.MeterName) // MassTransit Meter: https://masstransit.io/documentation/configuration/observability
                        .AddAzureMonitorMetricExporter(o =>
                        {
                            o.ConnectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                        });
                });
        }

        services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(Startup).Assembly);

        services.AddSingleton<ICache, Cache>();
        services.AddMemoryCache();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setupAction =>
        {
            setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = HeaderNames.Authorization,
                Description = "Bearer Authentication with JWT Token",
                Type = SecuritySchemeType.Http
            });
            setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
            setupAction.SwaggerDoc(
                "TestTemplate17OpenAPISpecification",
                new OpenApiInfo
                {
                    Title = "TestTemplate17 API",
                    Version = "v1",
                    Description = "This API allows access to TestTemplate17.",
                    Contact = new OpenApiContact
                    {
                        Name = "Author Name",
                        Url = new Uri("https://github.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://www.opensource.org/licenses/MIT")
                    },
                    TermsOfService = new Uri("https://www.my-terms-of-service.com")
                });

            setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TestTemplate17.Api.xml"));
        });

        services.AddCors(o => o.AddPolicy("All", builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders(Constants.Headers.Pagination);
        }));

        services.AddCors(o => o.AddPolicy("TestTemplate17Client", builder =>
        {
            var allowedOrigins = _configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>();
            builder
                .WithOrigins(allowedOrigins)
                .WithHeaders("Authorization", "Content-Type")
                .WithExposedHeaders(Constants.Headers.Pagination)
                .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete);
        }));

        services.AddMassTransit(x =>
        {
            if (string.IsNullOrEmpty(_configuration["MessageBroker"]))
            {
                x.UsingInMemory();
            }
            else
            {
                x.UsingAzureServiceBus((ctx, cfg) =>
                {
                    cfg.Host(_configuration["MessageBroker"]);

                    // Use the below line if you are not going with SetKebabCaseEndpointNameFormatter() above.
                    // Remember to configure the subscription endpoint accordingly (see WorkerServices Program.cs).
                    // cfg.Message<VoteCast>(configTopology => configTopology.SetEntityName("vote-cast-topic"));
                });
            }
            x.AddEntityFrameworkOutbox<TestTemplate17DbContext>(o =>
            {
                // configure which database lock provider to use (Postgres, SqlServer, or MySql)
                o.UseSqlServer();

                // enable the bus outbox
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(15);
            });
        });
        services.AddTestTemplate17ApplicationHandlers();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services
            .AddHealthChecks()
            .AddDbContextCheck<TestTemplate17DbContext>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
        app.UseHostLoggingMiddleware();

        // First use of Logging Exceptions.
        // This instance is here to catch and log any exceptions coming from middlewares
        // executed early in the pipeline.
        app.UseApiExceptionHandler(options =>
        {
            options.ApiErrorHandler = UpdateApiErrorResponse;
            options.LogLevelHandler = LogLevelHandler;
        });

        // Use headers forwarded by reverse proxy.
        app.UseForwardedHeaders();

        // if (env.IsProduction())
        // {
        //    app.UseHsts();
        // }
        app.UseCors("TestTemplate17Client");
        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/TestTemplate17OpenAPISpecification/swagger.json", "TestTemplate17 API");
            c.RoutePrefix = string.Empty;
        });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseUserLoggingMiddleware();

        // Second use of Logging Exceptions.
        // This instance is here to catch and log any exceptions coming from the controllers.
        // The reason for two logging middlewares is we can log user id and claims only
        // after .UseAuthentication() and .UseAuthorization() are executed. So the first
        // .UseApiExceptionHandler() has no access to user id and claims but has access to
        // machine name and thus at least provides some insight into any potential exceptions
        // coming from early in the pipeline. The second .UseApiExceptionHandler() has access
        // to machine name, user id and claims and can log any exceptions from the controllers.
        app.UseApiExceptionHandler(options =>
        {
            options.ApiErrorHandler = UpdateApiErrorResponse;
            options.LogLevelHandler = LogLevelHandler;
        });

        app.UseEndpoints(endpoints =>
        {
            // Liveness check does not include database connectivity check because even a transient
            // error will cause the orchestractor/load balancer to take the service down and restart it.
            // Readiness check includes database connectivity check to tell the orchestractor/load balancer
            // whether all the project dependencies are up and running.
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false, // No additional health checks.
            });
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponses.WriteJsonResponse
            });
            endpoints
                .MapGroup(Endpoints.Group.Api) // Common prefix for all endpoints.
                .MapEndpoints();
        });

        // Commented out as we are running front end as a standalone app.
        // app.UseSpa(spa =>
        // {
        //     spa.Options.SourcePath = "ClientApp";
        //     if (env.IsDevelopment())
        //     {
        //         // This is used if starting both front end and back end with the same command.
        //         // spa.UseReactDevelopmentServer(npmScript: "start");
        //         // This is used if starting front end separately from the back end, most likely to get better
        //         // separation. Faster hot reload when changing only front end and not having to go through front end
        //         // rebuild every time you change something on the back end.
        //         spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
        //     }
        // });
    }

    /// <summary>
    /// A demonstration of how returned message can be modified.
    /// </summary>
    private void UpdateApiErrorResponse(HttpContext context, Exception ex, ProblemDetails problemDetails)
    {
        // if (ex is LimitNotMappable)
        // {
        //     problemDetails.Detail = "A general error occurred.";
        // }
    }

    /// <summary>
    /// Define cases where a different log level is needed for logging exceptions.
    /// </summary>
    private LogLevel LogLevelHandler(HttpContext context, Exception ex) =>

        // if (ex is Exception)
        // {
        //     return LogLevel.Critical;
        // }
        // return LogLevel.Error;
        LogLevel.Critical;
}
