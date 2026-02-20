using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Cash;
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
        var register = BuildCashRegisterService(initialBalance);

        await register.Insert(amount);

        Assert.Equal(expectedBalance, await register.GetBalance());
    }

    [Fact]
    public async Task Insert_IncreasesBalance()
    {
        var register = BuildCashRegisterService();

        await register.Insert(2.50m);

        Assert.Equal(2.50m, await register.GetBalance());
    }

    [Fact]
    public async Task Charge_DecreasesBalance()
    {
        var register = BuildCashRegisterService(5.00m); 

        await register.Charge(1.25m);

        Assert.Equal(3.75m, await register.GetBalance());
    }

    

    private static CashRegisterService BuildCashRegisterService(decimal initialBalance = 0)
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(initialBalance);
        return BuildCashRegister(storage);
    }

    private static CashRegisterService BuildCashRegister(ICashStorage storage)
    {
        var services = new ServiceCollection();
        services.AddSingleton(storage);
        services.AddLogging();
        services.AddMediatR(typeof(CashRegisterService).Assembly);
        services.AddSingleton<CashRegisterService>();
        return services.BuildServiceProvider().GetRequiredService<CashRegisterService>();
    }
}
