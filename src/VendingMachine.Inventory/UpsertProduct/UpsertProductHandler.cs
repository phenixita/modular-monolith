using MediatR;

namespace VendingMachine.Inventory.UpsertProduct;

internal sealed class UpsertProductHandler(IInventoryRepository repository)
    : IRequestHandler<UpsertProductCommand, Unit>
{
    public async Task<Unit> Handle(UpsertProductCommand request, CancellationToken cancellationToken)
    {
        await repository.AddOrUpdateAsync(request.Product, cancellationToken);
        return Unit.Value;
    }
}
