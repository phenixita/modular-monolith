using MediatR;

namespace VendingMachine.Cash;

public sealed class GetBalanceHandler(ICashStorage storage)
    : IRequestHandler<GetBalanceQuery, decimal>
{
    public Task<decimal> Handle(GetBalanceQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(storage.GetBalance());
}
