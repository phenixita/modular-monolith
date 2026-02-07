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
