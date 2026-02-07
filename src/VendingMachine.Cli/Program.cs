using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;
using VendingMachine.Orders;

const string DefaultMongoConnection = "mongodb://root:root@localhost:27017/?authSource=admin";
const string DefaultMongoDatabase = "vendingmachine_inventory";
const string DefaultPostgresConnection =
    "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=vendingmachine";

var mongoConnection = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? DefaultMongoConnection;
var mongoDatabase = Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? DefaultMongoDatabase;
var postgresConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ?? DefaultPostgresConnection;

var services = new ServiceCollection();
services.AddSingleton<CashRegister>();
services.AddSingleton<IInventoryRepository>(_ => new MongoInventoryRepository(mongoConnection, mongoDatabase));
services.AddSingleton<IOrderRepository>(_ => new PostgresOrderRepository(postgresConnection));
services.AddMediatR(typeof(CashRegister).Assembly, typeof(InventoryCatalog).Assembly, typeof(OrderService).Assembly);

using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();
var orderRepository = provider.GetRequiredService<IOrderRepository>();
await orderRepository.EnsureCreatedAsync();
await SeedDataAsync(mediator);

Console.WriteLine("Vending Machine CLI");
Console.WriteLine("Commands: list, insert <amount>, buy <code>, balance, refund, help, exit");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLowerInvariant();

    try
    {
        switch (command)
        {
            case "list":
                await ListProductsAsync(mediator);
                break;
            case "insert":
                await InsertCashAsync(mediator, parts);
                break;
            case "buy":
                await BuyProductAsync(mediator, parts);
                break;
            case "balance":
                var balance = await mediator.Send(new GetBalanceQuery());
                Console.WriteLine($"Balance: {balance:0.00}");
                break;
            case "refund":
                var refund = await mediator.Send(new RefundAllCommand());
                Console.WriteLine(refund == 0 ? "No balance to refund." : $"Refunded {refund:0.00}");
                break;
            case "help":
                Console.WriteLine("Commands: list, insert <amount>, buy <code>, balance, refund, help, exit");
                break;
            case "exit":
                return;
            default:
                Console.WriteLine("Unknown command. Type 'help' for options.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

static async Task SeedDataAsync(IMediator mediator)
{
    var products = new[]
    {
        new Product("C01", "Espresso", 1.20m),
        new Product("C02", "Cappuccino", 1.50m),
        new Product("B01", "Water", 1.00m),
        new Product("S01", "Chips", 1.30m),
        new Product("S02", "Chocolate", 1.40m)
    };

    var stocks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["C01"] = 5,
        ["C02"] = 4,
        ["B01"] = 6,
        ["S01"] = 3,
        ["S02"] = 2
    };

    foreach (var product in products)
    {
        await mediator.Send(new UpsertProductCommand(product));
    }

    foreach (var (code, quantity) in stocks)
    {
        await mediator.Send(new SetStockCommand(code, quantity));
    }
}

static async Task ListProductsAsync(IMediator mediator)
{
    var products = await mediator.Send(new ListProductsQuery());
    foreach (var product in products)
    {
        var quantity = await mediator.Send(new GetStockQuery(product.Code));
        Console.WriteLine($"{product.Code} - {product.Name} ({product.Price:0.00}) [{quantity}]");
    }
}

static async Task InsertCashAsync(IMediator mediator, string[] parts)
{
    if (parts.Length < 2 || !decimal.TryParse(parts[1], out var amount))
    {
        Console.WriteLine("Usage: insert <amount>");
        return;
    }

    await mediator.Send(new InsertCashCommand(amount));
    var balance = await mediator.Send(new GetBalanceQuery());
    Console.WriteLine($"Inserted {amount:0.00}. Balance: {balance:0.00}");
}

static async Task BuyProductAsync(IMediator mediator, string[] parts)
{
    if (parts.Length < 2)
    {
        Console.WriteLine("Usage: buy <code>");
        return;
    }

    var result = await mediator.Send(new PurchaseProductCommand(parts[1]));
    switch (result.Status)
    {
        case OrderStatus.Success:
            Console.WriteLine($"Dispensed {result.Product.Name}. Enjoy!");
            break;
        case OrderStatus.OutOfStock:
        case OrderStatus.InsufficientFunds:
            Console.WriteLine(result.Message);
            break;
    }
}
