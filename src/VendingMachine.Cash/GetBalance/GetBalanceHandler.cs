using MediatR;

namespace VendingMachine.Cash.GetBalance;

internal sealed class GetBalanceHandler(ICashRepository repository)
    : IRequestHandler<GetBalanceQuery, decimal>
{
    public Task<decimal> Handle(GetBalanceQuery request, CancellationToken cancellationToken) =>
        repository.GetBalanceAsync(cancellationToken);
}
