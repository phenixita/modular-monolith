using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Inventory;

internal static class InventoryCommands
{
    public static System.CommandLine.Command Build(CliOptions options)
    {
        var inventoryCommand = new System.CommandLine.Command("inventory", "Manage catalog and stock.");
        inventoryCommand.AddCommand(BuildProductCommand(options));
        inventoryCommand.AddCommand(BuildStockCommand(options));

        return inventoryCommand;
    }

    private static System.CommandLine.Command BuildProductCommand(CliOptions options)
    {
        var productCommand = new System.CommandLine.Command("product", "Manage catalog products.");
        productCommand.AddCommand(BuildCreateProductCommand(options));

        return productCommand;
    }

    private static System.CommandLine.Command BuildCreateProductCommand(CliOptions options)
    {
        var createCommand = new System.CommandLine.Command("create",
            "Create a product in the catalog. Example: vm inventory product create COLA 2.00");

        var codeArgument = new Argument<string>("code", "Product code (e.g., COLA).");
        var priceArgument = new Argument<string>("price", "Product price with '.' as decimal separator (e.g., 2.00).");
        createCommand.AddArgument(codeArgument);
        createCommand.AddArgument(priceArgument);

        createCommand.SetHandler(async context =>
        {
            context.ExitCode = await ExecuteCreateProductAsync(context, options, codeArgument, priceArgument);
        });

        return createCommand;
    }

    private static System.CommandLine.Command BuildStockCommand(CliOptions options)
    {
        var stockCommand = new System.CommandLine.Command("stock", "Manage stock levels.");
        stockCommand.AddCommand(BuildLoadStockCommand(options));

        return stockCommand;
    }

    private static System.CommandLine.Command BuildLoadStockCommand(CliOptions options)
    {
        var loadCommand = new System.CommandLine.Command("load",
            "Load stock for a product. Example: vm inventory stock load COLA 5");

        var codeArgument = new Argument<string>("code", "Product code (e.g., COLA).");
        var quantityArgument = new Argument<string>("quantity", "Quantity to load (positive integer).");
        loadCommand.AddArgument(codeArgument);
        loadCommand.AddArgument(quantityArgument);

        loadCommand.SetHandler(async context =>
        {
            context.ExitCode = await ExecuteLoadStockAsync(context, options, codeArgument, quantityArgument);
        });

        return loadCommand;
    }

    private static Task<int> ExecuteCreateProductAsync(
        InvocationContext context,
        CliOptions options,
        Argument<string> codeArgument,
        Argument<string> priceArgument)
    {
        var format = CliCommandExecution.GetFormat(context, options);
        var configFile = CliCommandExecution.GetConfigFile(context, options);
        var code = context.ParseResult.GetValueForArgument(codeArgument);
        var priceText = context.ParseResult.GetValueForArgument(priceArgument);

        return CliCommandExecution.ExecuteAsync(async () =>
        {
            var price = CliParsing.ParseMoney(priceText, "price");
            var normalizedCode = CliParsing.EnsureCode(code);

            await ExecuteWithInventoryServiceAsync(configFile, async inventoryService =>
            {
                var product = new Product(normalizedCode, normalizedCode, price);
                await inventoryService.CreateProduct(product);

                CliOutputWriter.Write(format,
                    new { productCode = product.Code, price },
                    $"Product created: {product.Code} {CliParsing.FormatMoney(price)}");
            });
        }, context);
    }

    private static Task<int> ExecuteLoadStockAsync(
        InvocationContext context,
        CliOptions options,
        Argument<string> codeArgument,
        Argument<string> quantityArgument)
    {
        var format = CliCommandExecution.GetFormat(context, options);
        var configFile = CliCommandExecution.GetConfigFile(context, options);
        var code = context.ParseResult.GetValueForArgument(codeArgument);
        var quantityText = context.ParseResult.GetValueForArgument(quantityArgument);

        return CliCommandExecution.ExecuteAsync(async () =>
        {
            var quantity = CliParsing.ParseQuantity(quantityText, "quantity");
            var normalizedCode = CliParsing.EnsureCode(code);

            await ExecuteWithInventoryServiceAsync(configFile, async inventoryService =>
            {
                await inventoryService.AddStock(normalizedCode, quantity);
                var updatedStock = await inventoryService.GetStock(normalizedCode);

                CliOutputWriter.Write(format,
                    new { productCode = normalizedCode, stock = updatedStock },
                    $"Stock for {normalizedCode}: {updatedStock.ToString(CultureInfo.InvariantCulture)}");
            });
        }, context);
    }

    private static async Task ExecuteWithInventoryServiceAsync(
        FileInfo configFile,
        Func<IInventoryService, Task> action)
    {
        var config = CliConfigurationLoader.Load(configFile);
        using var provider = CliServiceProviderFactory.Build(config);
        using var scope = provider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        await action(inventoryService);
    }
}
