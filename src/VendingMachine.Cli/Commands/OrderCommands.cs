using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
using VendingMachine.Inventory;

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
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var normalizedCode = CliParsing.EnsureCode(code);
                var product = await mediator.Send(new GetProductByCodeQuery(normalizedCode));
                var stock = await mediator.Send(new GetStockQuery(normalizedCode));
                var balance = await mediator.Send(new GetBalanceQuery());

                if (stock <= 0)
                {
                    throw new InvalidOperationException($"Product {normalizedCode} is out of stock.");
                }

                if (balance < product.Price)
                {
                    throw new InvalidOperationException(
                        $"Insufficient balance. Price is {CliParsing.FormatMoney(product.Price)}, balance is {CliParsing.FormatMoney(balance)}.");
                }

                await mediator.Send(new ChargeCashCommand(product.Price));
                await mediator.Send(new RemoveStockCommand(normalizedCode, 1));

                var updatedBalance = await mediator.Send(new GetBalanceQuery());
                var updatedStock = await mediator.Send(new GetStockQuery(normalizedCode));

                CliOutputWriter.Write(format,
                    new
                    {
                        productCode = normalizedCode,
                        price = product.Price,
                        balance = updatedBalance,
                        stock = updatedStock
                    },
                    $"Order placed for {normalizedCode}.{Environment.NewLine}Balance: {CliParsing.FormatMoney(updatedBalance)}{Environment.NewLine}Stock: {updatedStock.ToString(CultureInfo.InvariantCulture)}");
            }, context);
        });

        return orderCommand;
    }
}
