using MediatR;

namespace VendingMachine.Inventory;

public sealed class UpsertProductHandler(IInventoryRepository repository)
    : IRequestHandler<UpsertProductCommand, Unit>
{
    public async Task<Unit> Handle(UpsertProductCommand request, CancellationToken cancellationToken)
    {
        await repository.AddOrUpdateAsync(request.Product, cancellationToken);
        return Unit.Value;
    }
}
