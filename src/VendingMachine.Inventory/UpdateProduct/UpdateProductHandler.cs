using MediatR;

namespace VendingMachine.Inventory.UpdateProduct;

internal sealed class UpdateProductHandler(IInventoryRepository repository)
    : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        EnsureValidPrice(request.Product.Price);

        await repository.GetByCodeAsync(request.Product.Code, cancellationToken);
        await repository.AddOrUpdateAsync(request.Product, cancellationToken);

        return Unit.Value;
    }

    private static void EnsureValidPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be zero or positive.");
        }
    }
}
