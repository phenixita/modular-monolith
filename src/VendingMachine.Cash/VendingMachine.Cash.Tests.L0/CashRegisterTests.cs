using VendingMachine.Cash;
using Xunit;

namespace VendingMachine.Cash.Tests.L0;

public sealed class CashRegisterTests
{
    [Fact]
    public void Insert_IncreasesBalance()
    {
        var register = new CashRegister();

        register.Insert(2.50m);

        Assert.Equal(2.50m, register.Balance);
    }

    [Fact]
    public void Charge_DecreasesBalance()
    {
        var register = new CashRegister();
        register.Insert(5.00m);

        register.Charge(1.25m);

        Assert.Equal(3.75m, register.Balance);
    }
}
