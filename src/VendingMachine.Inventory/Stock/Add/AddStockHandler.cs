using MediatR;

namespace VendingMachine.Inventory.Stock.Add;

internal sealed class AddStockHandler(IInventoryRepository repository)
    : IRequestHandler<AddStockCommand, Unit>
{
    public async Task<Unit> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        // var product = await repository.GetByCodeAsync(request.Code, cancellationToken);

        // if (product is null)
        // {
        //     throw new InvalidOperationException($"Product with code '{request.Code}' does not exist.");
        // }

        await repository.AddStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
