using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Inventory.Infrastructure;
using VendingMachine.Orders;
using VendingMachine.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddPersistence();
builder.Services.AddVendingMachineInventoryModule();
builder.Services.AddCashRegisterModule();
builder.Services.AddOrdersModule();

var infrastructureOptions = InfrastructureOptions.Load(builder.Configuration);

builder.Services.AddSingleton<IInventoryRepository>(_ =>
    new MongoInventoryRepository(infrastructureOptions.Mongo.ConnectionString, infrastructureOptions.Mongo.Database));

builder.Services.AddScoped<ICashStorage>(sp =>
{
    var connectionFactory = sp.GetRequiredService<IDbConnectionFactory>();
    return new PostgresCashStorage(connectionFactory);
});

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICashRegisterService, CashRegisterService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiEnvelope.Failure("internal_error", "An unexpected error occurred."));
            return;
        }

        var (statusCode, errorCode) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "validation_error"),
            FormatException => (StatusCodes.Status400BadRequest, "validation_error"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "not_found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "business_conflict"),
            _ => (StatusCodes.Status500InternalServerError, "internal_error")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(ApiEnvelope.Failure(errorCode, exception.Message));
    });
});

var api = app.MapGroup("/api");

api.MapPost("/inventory/products", async (
    CreateProductRequest request,
    IInventoryService inventoryService,
    CancellationToken cancellationToken) =>
{
    var code = NormalizeCode(request.Code);
    var product = new Product(code, code, request.Price);

    await inventoryService.CreateProduct(product);

    var data = new ProductCreatedResponse(product.Code, request.Price);
    return Results.Ok(ApiEnvelope.Success(data));
});

api.MapPost("/inventory/products/{code}/stock/load", async (
    string code,
    LoadStockRequest request,
    IInventoryService inventoryService,
    CancellationToken cancellationToken) =>
{
    var normalizedCode = NormalizeCode(code);

    await inventoryService.AddStock(normalizedCode, request.Quantity);
    var stock = await inventoryService.GetStock(normalizedCode);

    var data = new StockResponse(normalizedCode, stock);
    return Results.Ok(ApiEnvelope.Success(data));
});

api.MapPost("/cash/insert", async (
    InsertCashRequest request,
    ICashRegisterService cashService,
    CancellationToken cancellationToken) =>
{
    await cashService.Insert(request.Amount);
    var balance = await cashService.GetBalance();

    var data = new BalanceResponse(balance);
    return Results.Ok(ApiEnvelope.Success(data));
});

api.MapGet("/cash/balance", async (
    ICashRegisterService cashService,
    CancellationToken cancellationToken) =>
{
    var balance = await cashService.GetBalance();
    var data = new BalanceResponse(balance);

    return Results.Ok(ApiEnvelope.Success(data));
});

api.MapPost("/cash/refund", async (
    ICashRegisterService cashService,
    CancellationToken cancellationToken) =>
{
    var refunded = await cashService.RefundAll();
    var balance = await cashService.GetBalance();

    var data = new RefundResponse(refunded, balance);
    return Results.Ok(ApiEnvelope.Success(data));
});

api.MapPost("/orders", async (
    PlaceOrderRequest request,
    IOrderService orderService,
    CancellationToken cancellationToken) =>
{
    var code = NormalizeCode(request.Code);
    var receipt = await orderService.PlaceOrder(code, cancellationToken);

    var data = new PlaceOrderResponse(
        receipt.ProductCode,
        receipt.Price,
        receipt.Balance,
        receipt.Stock);

    return Results.Ok(ApiEnvelope.Success(data));
});

app.Run();

return;

static string NormalizeCode(string? code)
{
    if (string.IsNullOrWhiteSpace(code))
    {
        throw new ArgumentException("Product code is required.", nameof(code));
    }

    return code.Trim().ToUpperInvariant();
}

internal sealed record ApiEnvelope(object? Data, ApiError? Error)
{
    public static ApiEnvelope Success(object data) => new(data, null);

    public static ApiEnvelope Failure(string code, string message) =>
        new(null, new ApiError(code, message));
}

internal sealed record ApiError(string Code, string Message);

internal sealed record ProductCreatedResponse(string ProductCode, decimal Price);

internal sealed record StockResponse(string ProductCode, int Stock);

internal sealed record BalanceResponse(decimal Balance);

internal sealed record RefundResponse(decimal RefundedAmount, decimal Balance);

internal sealed record PlaceOrderResponse(string ProductCode, decimal Price, decimal Balance, int Stock);

internal sealed record CreateProductRequest(string? Code, decimal Price);

internal sealed record LoadStockRequest(int Quantity);

internal sealed record InsertCashRequest(decimal Amount);

internal sealed record PlaceOrderRequest(string? Code);

internal sealed record InfrastructureOptions(PostgresOptions Postgres, MongoOptions Mongo)
{
    public static InfrastructureOptions Load(IConfiguration configuration)
    {
        var postgresConnection = configuration["postgres:connectionString"];
        var mongoConnection = configuration["mongo:connectionString"];
        var mongoDatabase = configuration["mongo:database"];

        if (string.IsNullOrWhiteSpace(postgresConnection))
        {
            throw new InvalidOperationException("Missing configuration value 'postgres.connectionString'.");
        }

        if (string.IsNullOrWhiteSpace(mongoConnection))
        {
            throw new InvalidOperationException("Missing configuration value 'mongo.connectionString'.");
        }

        if (string.IsNullOrWhiteSpace(mongoDatabase))
        {
            throw new InvalidOperationException("Missing configuration value 'mongo.database'.");
        }

        return new InfrastructureOptions(
            new PostgresOptions(postgresConnection),
            new MongoOptions(mongoConnection, mongoDatabase));
    }
}

internal sealed record PostgresOptions(string ConnectionString);

internal sealed record MongoOptions(string ConnectionString, string Database);