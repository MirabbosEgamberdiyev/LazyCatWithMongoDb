using Application.Commens.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace Web.HandleException;
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError; // 500 if unexpected

        if (exception is UnauthorizedAccessException)
        {
            statusCode = HttpStatusCode.Unauthorized; // 401
        }
        else if (exception is ArgumentException || exception is ValidationException)
        {
            statusCode = HttpStatusCode.BadRequest; // 400
        }
        else if (exception is NotFoundException)
        {
            statusCode = HttpStatusCode.NotFound; // 404
        }

        var response = new { message = exception.Message };
        var payload = JsonSerializer.Serialize(response); // System.Text.Json kutubxonasini ishlatish
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(payload);
    }
}
