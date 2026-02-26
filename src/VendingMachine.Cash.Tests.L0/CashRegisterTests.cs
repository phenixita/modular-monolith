using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace VendingMachine.Cash.Tests.L0;

[Trait("Level", "L0")]
public sealed class CashRegisterTests
{
    [Theory]
    [InlineData(0, 2, 2)]
    [InlineData(2, 1, 3)]
    [InlineData(5, 5, 10)]
    public async Task Insert_IncreasesBalance_FromExamples(decimal initialBalance, decimal amount, decimal expectedBalance)
    {
        var cashRegisterService = await BuildCashRegisterService(initialBalance);

        await cashRegisterService.Insert(amount);

        Assert.Equal(expectedBalance, await cashRegisterService.GetBalance());
    }

    [Fact]
    public async Task Insert_IncreasesBalance()
    {
        var cashRegisterService = await BuildCashRegisterService();

        await cashRegisterService.Insert(2.50m);

        Assert.Equal(2.50m, await cashRegisterService.GetBalance());
    }

    [Fact]
    public async Task Charge_DecreasesBalance()
    {
        var cashRegisterService = await BuildCashRegisterService(5.00m);

        await cashRegisterService.Charge(1.25m);

        Assert.Equal(3.75m, await cashRegisterService.GetBalance());
    }



    private static async Task<ICashRegisterService> BuildCashRegisterService(decimal initialBalance = 0)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddCashRegisterModuleForTests()
            .BuildServiceProvider();

        var repository = serviceProvider.GetRequiredService<ICashRepository>();
        
        await repository.SetBalanceAsync(initialBalance);

        return serviceProvider.GetRequiredService<ICashRegisterService>();
    }
}
