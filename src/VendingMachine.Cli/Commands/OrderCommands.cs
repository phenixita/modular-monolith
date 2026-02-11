using System.CommandLine;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Orders;

internal static class OrderCommands
{
    public static Command Build(CliOptions options)
    {
        var orderCommand = new Command("order", "Place an order. Example: vm order COLA");
        var codeArg = new Argument<string>("code", "Product code (e.g., COLA).");
        orderCommand.AddArgument(codeArg);

        orderCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);
            var code = context.ParseResult.GetValueForArgument(codeArg);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                var normalizedCode = CliParsing.EnsureCode(code);
                var receipt = await orderService.PlaceOrder(normalizedCode);

                CliOutputWriter.Write(format,
                    new
                    {
                        productCode = receipt.ProductCode,
                        price = receipt.Price,
                        balance = receipt.Balance,
                        stock = receipt.Stock
                    },
                    $"Order placed for {receipt.ProductCode}.{Environment.NewLine}Balance: {CliParsing.FormatMoney(receipt.Balance)}{Environment.NewLine}Stock: {receipt.Stock.ToString(CultureInfo.InvariantCulture)}");
            }, context);
        });

        return orderCommand;
    }
}
