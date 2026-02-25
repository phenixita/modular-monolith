using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VendingMachine.Reporting;

internal static class LoggingHelper
{
    public static async Task<T> ExecuteWithLoggingAsync<T>(
        ILogger logger,
        string operationName,
        Func<Task<T>> operation,
        LogLevel successLevel = LogLevel.Debug,
        object? parameters = null)
    {
        var sw = Stopwatch.StartNew();

        if (parameters != null)
            logger.LogTrace("{Operation} parameters {@Parameters}", operationName, parameters);

        logger.Log(successLevel, "{Operation} starting", operationName);

        try
        {
            var result = await operation();

            logger.Log(successLevel, "{Operation} completed in {ElapsedMs}ms with result {@Result}",
                operationName, sw.ElapsedMilliseconds, result);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Operation} failed after {ElapsedMs}ms",
                operationName, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public static async Task ExecuteWithLoggingAsync(
        ILogger logger,
        string operationName,
        Func<Task> operation,
        LogLevel successLevel = LogLevel.Debug,
        object? parameters = null)
    {
        var sw = Stopwatch.StartNew();

        if (parameters != null)
            logger.LogTrace("{Operation} parameters {@Parameters}", operationName, parameters);

        logger.Log(successLevel, "{Operation} starting", operationName);

        try
        {
            await operation();

            logger.Log(successLevel, "{Operation} completed in {ElapsedMs}ms",
                operationName, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Operation} failed after {ElapsedMs}ms",
                operationName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
