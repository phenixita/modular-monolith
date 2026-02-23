using MediatR;

namespace VendingMachine.Cash.RefundAll;

internal sealed class RefundAllHandler(ICashStorage storage)
    : IRequestHandler<RefundAllCommand, decimal>
{
    public async Task<decimal> Handle(RefundAllCommand request, CancellationToken cancellationToken)
    {
        var refund = await storage.GetBalanceAsync(cancellationToken);
        await storage.SetBalanceAsync(0m, cancellationToken);
        return refund;
    }
}
