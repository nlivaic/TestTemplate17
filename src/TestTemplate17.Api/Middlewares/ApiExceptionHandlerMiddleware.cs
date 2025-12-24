using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestTemplate17.Api.Helpers;
using TestTemplate17.Common.Exceptions;

namespace TestTemplate17.Api.Middlewares;

/// <summary>
/// Handles exceptions by logging the exception object.
/// </summary>
public class ApiExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlerMiddleware> _logger;
    private readonly ApiExceptionHandlerOptions _options;

    public ApiExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionHandlerMiddleware> logger,
        ApiExceptionHandlerOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            HandleError(context, ex);
        }
    }

    private static async Task HandleBusinessException(HttpContext context, Exception ex)
    {
        var validationProblemDetails = ProblemDetailsFactory
                .Create(context, new Dictionary<string, string[]>
                {
                    { string.Empty, new string[] { ex.Message } }
                });
        var result = JsonSerializer.Serialize(validationProblemDetails);
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = validationProblemDetails.Status!.Value;
        await context.Response.WriteAsync(result);
    }

    private static async Task HandleEntityNotFoundException(HttpContext context, Exception ex)
    {
        var problemDetails = ProblemDetailsFactory.CreateNotFoundProblemDetails(context, ex.Message);
        var result = JsonSerializer.Serialize(problemDetails);
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status!.Value;
        await context.Response.WriteAsync(result);
    }

    private static async Task HandleDbUpdateConcurrencyExceptionException(HttpContext context, Exception ex)
    {
        var problemDetails = ProblemDetailsFactory
                .CreateDbConcurrencyUpdateProblemDetails(context);
        var result = JsonSerializer.Serialize(problemDetails);
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status!.Value;
        await context.Response.WriteAsync(result);
    }

    private async void HandleError(HttpContext context, Exception ex)
    {
        if (ex is BusinessException)
        {
            await HandleBusinessException(context, ex);
        }
        else if (ex is EntityNotFoundException)
        {
            await HandleEntityNotFoundException(context, ex);
        }
        else if (ex is DbUpdateConcurrencyException)
        {
            await HandleDbUpdateConcurrencyExceptionException(context, ex);
        }
        else
        {
            await HandleException(context, ex);
        }
    }

    private async Task HandleException(HttpContext context, Exception ex)
    {
        var innermostException = GetInnermostException(ex);
        var problemDetail = ProblemDetailsFactory.CreateInternalServerErrorProblemDetails(context);

        _options.ApiErrorHandler?.Invoke(context, ex, problemDetail);
        var logLevel = _options.LogLevelHandler?.Invoke(context, ex) ?? LogLevel.Error;
        if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
        {
            string body = string.Empty;
            using (var reader = new StreamReader(context.Request.Body))
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                body = await reader.ReadToEndAsync();
            }
            _logger.Log(
                logLevel,
                innermostException,
                innermostException.Message + " -- {TraceId} -- {Headers} -- {Body}",
                problemDetail.Extensions["traceId"],
                string.Join(
                    Environment.NewLine,
                    context.Request.Headers.Select(h => h.Key != "Authorization" ? $"{h.Key}={h.Value}" : $"{h.Key}=****")),
                body);
        }
        else
        {
            _logger.Log(
                logLevel,
                innermostException,
                innermostException.Message + " -- {TraceId}",
                problemDetail.Extensions["traceId"]);
        }
        var result = JsonSerializer.Serialize(problemDetail);
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(result);
    }

    private Exception GetInnermostException(Exception ex) =>
        ex.InnerException != null
            ? GetInnermostException(ex.InnerException)
            : ex;
}
