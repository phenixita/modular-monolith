using MediatR;

namespace VendingMachine.Cash.GetBalance;

internal sealed class GetBalanceHandler(ICashStorage storage)
    : IRequestHandler<GetBalanceQuery, decimal>
{
    public async Task<decimal> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        return await storage.GetBalanceAsync(cancellationToken);
    }
}
