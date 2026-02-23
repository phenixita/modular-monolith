using Microsoft.AspNetCore.Diagnostics;
using VendingMachine.Api.Contracts;

namespace VendingMachine.Api.Configuration;

internal static class ExceptionHandlingExtensions
{
    public static WebApplication UseApiExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception is null)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(
                        ApiEnvelope.Failure("internal_error", "An unexpected error occurred."));
                    return;
                }

                var (statusCode, errorCode) = MapException(exception);
                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(ApiEnvelope.Failure(errorCode, exception.Message));
            });
        });

        return app;
    }

    private static (int StatusCode, string ErrorCode) MapException(Exception exception) =>
        exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "validation_error"),
            FormatException => (StatusCodes.Status400BadRequest, "validation_error"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "not_found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "business_conflict"),
            _ => (StatusCodes.Status500InternalServerError, "internal_error")
        };
}
