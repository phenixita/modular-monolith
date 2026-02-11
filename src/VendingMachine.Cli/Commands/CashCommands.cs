using System.CommandLine;
using System.CommandLine.Invocation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;

internal static class CashCommands
{
    public static Command Build(CliOptions options)
    {
        var cashCommand = new Command("cash", "Manage cash balance.");

        var insertCommand = new Command("insert", "Insert cash. Example: vm cash insert 2.00");
        var amountArg = new Argument<string>("amount", "Amount with '.' as decimal separator (e.g., 2.00).");
        insertCommand.AddArgument(amountArg);

        insertCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);
            var amountText = context.ParseResult.GetValueForArgument(amountArg);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var amount = CliParsing.ParseMoney(amountText, "amount");
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new InsertCashCommand(amount));
                var balance = await mediator.Send(new GetBalanceQuery());

                CliOutputWriter.Write(format,
                    new { balance },
                    $"Balance: {CliParsing.FormatMoney(balance)}");
            }, context);
        });

        var balanceCommand = new Command("balance", "Show current balance. Example: vm cash balance");
        balanceCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var balance = await mediator.Send(new GetBalanceQuery());
                CliOutputWriter.Write(format,
                    new { balance },
                    $"Balance: {CliParsing.FormatMoney(balance)}");
            }, context);
        });

        var refundCommand = new Command("refund", "Refund all remaining credit. Example: vm cash refund");
        refundCommand.SetHandler(async context =>
        {
            var format = CliCommandExecution.GetFormat(context, options);
            var configFile = CliCommandExecution.GetConfigFile(context, options);

            context.ExitCode = await CliCommandExecution.ExecuteAsync(async () =>
            {
                var config = CliConfigurationLoader.Load(configFile);
                using var provider = CliServiceProviderFactory.Build(config);
                using var scope = provider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var refunded = await mediator.Send(new RefundAllCommand());
                var balance = await mediator.Send(new GetBalanceQuery());

                CliOutputWriter.Write(format,
                    new { refundedAmount = refunded, balance },
                    $"Refunded: {CliParsing.FormatMoney(refunded)}{Environment.NewLine}Balance: {CliParsing.FormatMoney(balance)}");
            }, context);
        });

        cashCommand.AddCommand(insertCommand);
        cashCommand.AddCommand(balanceCommand);
        cashCommand.AddCommand(refundCommand);

        return cashCommand;
    }
}
