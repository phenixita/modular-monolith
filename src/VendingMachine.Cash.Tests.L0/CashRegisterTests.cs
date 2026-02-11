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
    public void Insert_IncreasesBalance_FromExamples(decimal initialBalance, decimal amount, decimal expectedBalance)
    {
        var storage = new InMemoryCashStorage();
        storage.SetBalance(initialBalance);
        var register = new CashRegister(storage);

        register.Insert(amount);

        Assert.Equal(expectedBalance, register.Balance);
    }

    [Fact]
    public void Insert_IncreasesBalance()
    {
        var register = new CashRegister(new InMemoryCashStorage());

        register.Insert(2.50m);

        Assert.Equal(2.50m, register.Balance);
    }

    [Fact]
    public void Charge_DecreasesBalance()
    {
        var register = new CashRegister(new InMemoryCashStorage());
        register.Insert(5.00m);

        register.Charge(1.25m);

        Assert.Equal(3.75m, register.Balance);
    }
}
