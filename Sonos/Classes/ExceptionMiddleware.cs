using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using HomeLogging;
using Microsoft.AspNetCore.Http;

namespace Sonos.Classes;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogging _logger;
    public ExceptionMiddleware(RequestDelegate next, ILogging logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        //context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            Success = false
        };
        Boolean log = true;
        switch (exception)
        {
            case OperationCanceledException:
                log = false;
                break;
        }
        if(log)
        _logger.ServerErrorsAdd("Request:"+context.Request.Path, exception, "ExceptionMiddleWare");
        var result = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(result);
    }
}