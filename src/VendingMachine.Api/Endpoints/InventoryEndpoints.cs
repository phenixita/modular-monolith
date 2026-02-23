using VendingMachine.Api.Contracts;
using VendingMachine.Api.Shared;
using VendingMachine.Inventory;

namespace VendingMachine.Api.Endpoints;

internal static class InventoryEndpoints
{
    public static RouteGroupBuilder MapInventoryEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/inventory/products", CreateProductAsync);
        api.MapPost("/inventory/products/{code}/stock/load", LoadStockAsync);

        return api;
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        IInventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var code = ProductCodeNormalizer.Normalize(request.Code);
        var product = new Product(code, code, request.Price);

        await inventoryService.CreateProduct(product, cancellationToken);

        var data = new ProductCreatedResponse(product.Code, request.Price);
        return Results.Ok(ApiEnvelope.Success(data));
    }

    private static async Task<IResult> LoadStockAsync(
        string code,
        LoadStockRequest request,
        IInventoryService inventoryService,
        CancellationToken cancellationToken)
    {
        var normalizedCode = ProductCodeNormalizer.Normalize(code);

        await inventoryService.AddStock(normalizedCode, request.Quantity, cancellationToken);
        var stock = await inventoryService.GetStock(normalizedCode, cancellationToken);

        var data = new StockResponse(normalizedCode, stock);
        return Results.Ok(ApiEnvelope.Success(data));
    }
}
