using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Inventory;

internal static class InventoryCommands
{
    public static Command Build(CliOptions options)
    {
        var inventoryCommand = new Command("inventory", "Manage catalog and stock.");

        var productCommand = new Command("product", "Manage catalog products.");
        var createCommand = new Command("create",
            "Create a product in the catalog. Example: vm inventory product create COLA 2.00");

        var codeArg = new Argument<string>("code", "Product code (e.g., COLA).");
        var priceArg = new Argument<string>("price", "Product price with '.' as decimal separator (e.g., 2.00).");
        createCommand.AddArgument(codeArg);
        createCommand.AddArgument(priceArg);

        createCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);
            var code = context.ParseResult.GetValueForArgument(codeArg);
            var priceText = context.ParseResult.GetValueForArgument(priceArg);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var price = CliParsing.ParseMoney(priceText, "price");
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var normalizedCode = CliParsing.EnsureCode(code);
                var product = new Product(normalizedCode, normalizedCode, price);
                await mediator.Send(new CreateProductCommand(product));

                CliOutputWriter.Write(format,
                    new { productCode = product.Code, price },
                    $"Product created: {product.Code} {CliParsing.FormatMoney(price)}");
            }, context);
        });

        productCommand.AddCommand(createCommand);

        var stockCommand = new Command("stock", "Manage stock levels.");
        var loadCommand = new Command("load",
            "Load stock for a product. Example: vm inventory stock load COLA 5");

        var loadCodeArg = new Argument<string>("code", "Product code (e.g., COLA).");
        var quantityArg = new Argument<string>("quantity", "Quantity to load (positive integer).");
        loadCommand.AddArgument(loadCodeArg);
        loadCommand.AddArgument(quantityArg);

        loadCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);
            var code = context.ParseResult.GetValueForArgument(loadCodeArg);
            var quantityText = context.ParseResult.GetValueForArgument(quantityArg);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var quantity = CliParsing.ParseQuantity(quantityText, "quantity");
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var normalizedCode = CliParsing.EnsureCode(code);
                await mediator.Send(new AddStockCommand(normalizedCode, quantity));
                var updatedStock = await mediator.Send(new GetStockQuery(normalizedCode));

                CliOutputWriter.Write(format,
                    new { productCode = normalizedCode, stock = updatedStock },
                    $"Stock for {normalizedCode}: {updatedStock.ToString(CultureInfo.InvariantCulture)}");
            }, context);
        });

        stockCommand.AddCommand(loadCommand);

        inventoryCommand.AddCommand(productCommand);
        inventoryCommand.AddCommand(stockCommand);

        return inventoryCommand;
    }
}
