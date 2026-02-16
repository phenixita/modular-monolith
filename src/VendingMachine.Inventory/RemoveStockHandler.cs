using MediatR;

namespace VendingMachine.Inventory;

internal sealed class RemoveStockHandler(IInventoryRepository repository)
    : IRequestHandler<RemoveStockCommand, Unit>
{
    public async Task<Unit> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
