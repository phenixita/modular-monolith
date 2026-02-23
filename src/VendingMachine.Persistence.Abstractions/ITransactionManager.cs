namespace VendingMachine.Persistence;

/// <summary>
/// Manages database transactions across operations.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Begins a new unit of work with the default isolation level (Serializable).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new unit of work.</returns>
    Task<IUnitOfWork> BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation within a transaction scope.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<IUnitOfWork, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation within a transaction scope without returning a result.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteInTransactionAsync(
        Func<IUnitOfWork, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
