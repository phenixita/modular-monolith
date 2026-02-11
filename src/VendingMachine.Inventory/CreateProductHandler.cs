using MediatR;

namespace VendingMachine.Inventory;

public sealed class CreateProductHandler(IInventoryRepository repository)
    : IRequestHandler<CreateProductCommand, Unit>
{
    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        EnsureValidPrice(request.Product.Price);
        await EnsureProductDoesNotExist(request.Product.Code, cancellationToken);
        await repository.AddOrUpdateAsync(request.Product, cancellationToken);
        return Unit.Value;
    }

    private async Task EnsureProductDoesNotExist(string code, CancellationToken cancellationToken)
    {
        try
        {
            await repository.GetByCodeAsync(code, cancellationToken);
            throw new InvalidOperationException("Product already exists.");
        }
        catch (KeyNotFoundException)
        {
        }
    }

    private static void EnsureValidPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be zero or positive.");
        }
    }
}
