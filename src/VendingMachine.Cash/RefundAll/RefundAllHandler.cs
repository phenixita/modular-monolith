using MediatR;

namespace VendingMachine.Cash.RefundAll;

internal sealed class RefundAllHandler(ICashStorage storage)
    : IRequestHandler<RefundAllCommand, decimal>
{
    public Task<decimal> Handle(RefundAllCommand request, CancellationToken cancellationToken)
    {
        var refund = storage.GetBalance();
        storage.SetBalance(0m);
        return Task.FromResult(refund);
    }
}
