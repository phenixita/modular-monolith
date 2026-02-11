using MediatR;

namespace VendingMachine.Inventory;

public sealed class DeleteProductHandler(IInventoryRepository repository)
    : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var quantity = await repository.GetQuantityAsync(request.Code, cancellationToken);
        if (quantity > 0)
        {
            throw new InvalidOperationException("Product must have zero stock to be deleted.");
        }

        await repository.DeleteAsync(request.Code, cancellationToken);
        return Unit.Value;
    }
}
