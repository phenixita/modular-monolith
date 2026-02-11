using VendingMachine.Cash;
using Xunit;

namespace VendingMachine.Cash.Tests.L1;

[Trait("Level", "L1")]
public sealed class CashRegisterInfrastructureTests
{
    [Fact]
    public async Task CashRegister_PersistsBalanceAcrossInstances_WithPostgresStorage()
    {
        var fixture = new InfrastructureFixture();
        await fixture.InitializeAsync();
        try
        {
            var storage1 = new PostgresCashStorage(fixture.ConnectionString);
            storage1.EnsureCreated();
            storage1.SetBalance(0m);

            var register1 = new CashRegister(storage1);
            register1.Insert(2.00m);
            register1.Charge(1.25m);

            Assert.Equal(0.75m, register1.Balance);

            var register2 = new CashRegister(new PostgresCashStorage(fixture.ConnectionString));
            Assert.Equal(0.75m, register2.Balance);

            var refunded = register2.RefundAll();
            Assert.Equal(0.75m, refunded);

            var register3 = new CashRegister(new PostgresCashStorage(fixture.ConnectionString));
            Assert.Equal(0m, register3.Balance);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }
}
