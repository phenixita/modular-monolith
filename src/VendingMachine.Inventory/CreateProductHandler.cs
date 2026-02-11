using MediatR;

namespace VendingMachine.Inventory;

public sealed class CreateProductHandler(IInventoryRepository repository)
    : IRequestHandler<CreateProductCommand, Unit>
{
    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        EnsureValidPrice(request.Product.Price);

        try
        {
            await repository.GetByCodeAsync(request.Product.Code, cancellationToken);
            throw new InvalidOperationException("Product already exists.");
        }
        catch (KeyNotFoundException)
        {
            // Expected when the product does not exist.
        }

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
