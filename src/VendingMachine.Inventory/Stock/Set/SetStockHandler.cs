using MediatR;

namespace VendingMachine.Inventory.Stock.Set;

internal sealed class SetStockHandler(IInventoryRepository repository)
    : IRequestHandler<SetStockCommand, Unit>
{
    public async Task<Unit> Handle(SetStockCommand request, CancellationToken cancellationToken)
    {
        await repository.SetStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
