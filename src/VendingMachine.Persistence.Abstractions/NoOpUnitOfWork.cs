namespace VendingMachine.Persistence;

public sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
        action(cancellationToken);

    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default) =>
        action(cancellationToken);
}
