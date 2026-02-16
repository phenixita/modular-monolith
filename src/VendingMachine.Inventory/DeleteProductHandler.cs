using MediatR;

namespace VendingMachine.Inventory;

internal sealed class DeleteProductHandler(IInventoryRepository repository)
    : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var quantity = await FetchProductQuantity(request.Code, cancellationToken);
        EnsureProductStockIsEmpty(quantity);
        await repository.DeleteAsync(request.Code, cancellationToken);
        return Unit.Value;
    }

    private async Task<int> FetchProductQuantity(string code, CancellationToken cancellationToken) =>
        await repository.GetQuantityAsync(code, cancellationToken);

    private static void EnsureProductStockIsEmpty(int quantity)
    {
        if (quantity > 0)
        {
            throw new InvalidOperationException("Product must have zero stock to be deleted.");
        }
    }
}
