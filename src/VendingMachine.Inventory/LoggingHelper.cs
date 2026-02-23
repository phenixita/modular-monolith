using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VendingMachine.Inventory._Infrastructure;

/// <summary>
/// Internal helper to encapsulate logging pattern across service operations.
/// Implements DRY principle to avoid code duplication in service layer.
/// </summary>
internal static class LoggingHelper
{
    /// <summary>
    /// Executes an async operation with comprehensive logging (entry, timing, result, errors).
    /// </summary>
    /// <typeparam name="T">The return type of the operation</typeparam>
    /// <param name="logger">The logger instance</param>
    /// <param name="operationName">The name of the operation (e.g., "InventoryService.GetProduct")</param>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="successLevel">Log level for successful operations (default: Debug)</param>
    /// <param name="parameters">Optional parameters to log at Trace level</param>
    /// <returns>The result of the operation</returns>
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

    /// <summary>
    /// Executes an async void operation with comprehensive logging.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="operationName">The name of the operation</param>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="successLevel">Log level for successful operations (default: Debug)</param>
    /// <param name="parameters">Optional parameters to log at Trace level</param>
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
