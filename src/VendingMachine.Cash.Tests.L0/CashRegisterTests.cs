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
        var storage = new InMemoryCashStorage();
        storage.SetBalance(initialBalance);
        var register = BuildRegister(storage);

        await register.Insert(amount);

        Assert.Equal(expectedBalance, register.Balance);
    }

    [Fact]
    public async Task Insert_IncreasesBalance()
    {
        var register = BuildRegister(new InMemoryCashStorage());

        await register.Insert(2.50m);

        Assert.Equal(2.50m, register.Balance);
    }

    [Fact]
    public async Task Charge_DecreasesBalance()
    {
        var register = BuildRegister(new InMemoryCashStorage());
        
        await register.Insert(5.00m);

        await register.Charge(1.25m);

        Assert.Equal(3.75m, register.Balance);
    }

    private static CashRegisterService BuildRegister(ICashStorage storage)
    {
        var services = new ServiceCollection();
        services.AddSingleton(storage);
        services.AddMediatR(typeof(CashRegisterService).Assembly);
        services.AddSingleton<CashRegisterService>();
        return services.BuildServiceProvider().GetRequiredService<CashRegisterService>();
    }
}
