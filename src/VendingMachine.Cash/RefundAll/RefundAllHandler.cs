using MediatR;

namespace VendingMachine.Cash.RefundAll;

internal sealed class RefundAllHandler(ICashRepository repository)
    : IRequestHandler<RefundAllCommand, decimal>
{
    public async Task<decimal> Handle(RefundAllCommand request, CancellationToken cancellationToken)
    {
        var refund = await repository.GetBalanceAsync(cancellationToken);
        await repository.SetBalanceAsync(0m, cancellationToken);
        return refund;
    }
}
