using MediatR;

namespace VendingMachine.Inventory;

public sealed class InventoryService : IInventoryService
{
    private readonly IMediator _mediator;

    public InventoryService(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task CreateProduct(Product product) =>
        await _mediator.Send(new CreateProductCommand(product));

    public async Task UpdateProduct(Product product) =>
        await _mediator.Send(new UpdateProductCommand(product));

    public async Task DeleteProduct(string code) =>
        await _mediator.Send(new DeleteProductCommand(code));

    public async Task UpsertProduct(Product product) =>
        await _mediator.Send(new UpsertProductCommand(product));

    public async Task<Product> GetProductByCode(string code) =>
        await _mediator.Send(new GetProductByCodeQuery(code));

    public async Task<IReadOnlyCollection<Product>> ListProducts() =>
        await _mediator.Send(new ListProductsQuery());

    public async Task AddStock(string code, int quantity) =>
        await _mediator.Send(new AddStockCommand(code, quantity));

    public async Task RemoveStock(string code, int quantity) =>
        await _mediator.Send(new RemoveStockCommand(code, quantity));

    public async Task SetStock(string code, int quantity) =>
        await _mediator.Send(new SetStockCommand(code, quantity));

    public async Task<int> GetStock(string code) =>
        await _mediator.Send(new GetStockQuery(code));
}
