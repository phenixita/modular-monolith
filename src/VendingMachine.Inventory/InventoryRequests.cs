using MediatR;

namespace VendingMachine.Inventory;

public sealed record ListProductsQuery() : IRequest<IReadOnlyCollection<Product>>;

public sealed class ListProductsHandler(IInventoryRepository repository)
    : IRequestHandler<ListProductsQuery, IReadOnlyCollection<Product>>
{
    public Task<IReadOnlyCollection<Product>> Handle(ListProductsQuery request, CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}

public sealed record GetProductByCodeQuery(string Code) : IRequest<Product>;

public sealed class GetProductByCodeHandler(IInventoryRepository repository)
    : IRequestHandler<GetProductByCodeQuery, Product>
{
    public Task<Product> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken) =>
        repository.GetByCodeAsync(request.Code, cancellationToken);
}

public sealed record GetStockQuery(string Code) : IRequest<int>;

public sealed class GetStockHandler(IInventoryRepository repository)
    : IRequestHandler<GetStockQuery, int>
{
    public Task<int> Handle(GetStockQuery request, CancellationToken cancellationToken) =>
        repository.GetQuantityAsync(request.Code, cancellationToken);
}

public sealed record UpsertProductCommand(Product Product) : IRequest<Unit>;

public sealed class UpsertProductHandler(IInventoryRepository repository)
    : IRequestHandler<UpsertProductCommand, Unit>
{
    public async Task<Unit> Handle(UpsertProductCommand request, CancellationToken cancellationToken)
    {
        await repository.AddOrUpdateAsync(request.Product, cancellationToken);
        return Unit.Value;
    }
}

public sealed record CreateProductCommand(Product Product) : IRequest<Unit>;

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

public sealed record UpdateProductCommand(Product Product) : IRequest<Unit>;

public sealed class UpdateProductHandler(IInventoryRepository repository)
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

public sealed record DeleteProductCommand(string Code) : IRequest<Unit>;

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

public sealed record AddStockCommand(string Code, int Quantity) : IRequest<Unit>;

public sealed class AddStockHandler(IInventoryRepository repository)
    : IRequestHandler<AddStockCommand, Unit>
{
    public async Task<Unit> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        await repository.AddStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}

public sealed record RemoveStockCommand(string Code, int Quantity) : IRequest<Unit>;

public sealed class RemoveStockHandler(IInventoryRepository repository)
    : IRequestHandler<RemoveStockCommand, Unit>
{
    public async Task<Unit> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}

public sealed record SetStockCommand(string Code, int Quantity) : IRequest<Unit>;

public sealed class SetStockHandler(IInventoryRepository repository)
    : IRequestHandler<SetStockCommand, Unit>
{
    public async Task<Unit> Handle(SetStockCommand request, CancellationToken cancellationToken)
    {
        await repository.SetStockAsync(request.Code, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
