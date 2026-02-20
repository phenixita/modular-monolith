using MediatR;
using Microsoft.Extensions.Logging;
using VendingMachine.Inventory._Infrastructure;
using VendingMachine.Inventory.CreateProduct;
using VendingMachine.Inventory.DeleteProduct;
using VendingMachine.Inventory.GetProduct; 
using VendingMachine.Inventory.ListProducts;
using VendingMachine.Inventory.RemoveStock;
using VendingMachine.Inventory.Stock.Add;
using VendingMachine.Inventory.Stock.Get;
using VendingMachine.Inventory.Stock.Set;
using VendingMachine.Inventory.UpdateProduct;
using VendingMachine.Inventory.UpsertProduct;

namespace VendingMachine.Inventory;

public sealed class InventoryService : IInventoryService
{
    private readonly IMediator _mediator;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IMediator mediator, ILogger<InventoryService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task CreateProduct(Product product) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.CreateProduct",
            () => _mediator.Send(new CreateProductCommand(product)),
            parameters: new { Product = product });

    public Task UpdateProduct(Product product) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.UpdateProduct",
            () => _mediator.Send(new UpdateProductCommand(product)),
            parameters: new { Product = product });

    public Task DeleteProduct(string code) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.DeleteProduct",
            () => _mediator.Send(new DeleteProductCommand(code)),
            parameters: new { Code = code });

    public Task UpsertProduct(Product product) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.UpsertProduct",
            () => _mediator.Send(new UpsertProductCommand(product)),
            parameters: new { Product = product });

    public Task<Product> GetProductByCode(string code) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.GetProductByCode",
            () => _mediator.Send(new GetProductByCodeQuery(code)),
            parameters: new { Code = code });

    public Task<IReadOnlyCollection<Product>> ListProducts() =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.ListProducts",
            () => _mediator.Send(new ListProductsQuery()));

    public Task AddStock(string code, int quantity) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.AddStock",
            () => _mediator.Send(new AddStockCommand(code, quantity)),
            parameters: new { Code = code, Quantity = quantity });

    public Task RemoveStock(string code, int quantity) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.RemoveStock",
            () => _mediator.Send(new RemoveStockCommand(code, quantity)),
            parameters: new { Code = code, Quantity = quantity });

    public Task SetStock(string code, int quantity) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.SetStock",
            () => _mediator.Send(new SetStockCommand(code, quantity)),
            parameters: new { Code = code, Quantity = quantity });

    public Task<int> GetStock(string code) =>
        LoggingHelper.ExecuteWithLoggingAsync(
            _logger,
            "InventoryService.GetStock",
            () => _mediator.Send(new GetStockQuery(code)),
            parameters: new { Code = code });
}
