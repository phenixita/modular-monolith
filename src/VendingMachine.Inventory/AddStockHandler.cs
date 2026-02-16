using MediatR;

namespace VendingMachine.Inventory;

internal sealed class AddStockHandler(IInventoryRepository repository)
    : IRequestHandler<AddStockCommand, Unit>
{
    public async Task<Unit> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        await repository.AddStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
