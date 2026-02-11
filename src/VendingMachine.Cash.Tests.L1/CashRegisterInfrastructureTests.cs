using MediatR;
using Microsoft.Extensions.DependencyInjection;
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

            var register1 = BuildRegister(storage1);
            await register1.Insert(2.00m);
            await register1.Charge(1.25m);

            Assert.Equal(0.75m, register1.Balance);

            var register2 = BuildRegister(new PostgresCashStorage(fixture.ConnectionString));
            Assert.Equal(0.75m, register2.Balance);

            var refunded = await register2.RefundAll();
            Assert.Equal(0.75m, refunded);

            var register3 = BuildRegister(new PostgresCashStorage(fixture.ConnectionString));
            Assert.Equal(0m, register3.Balance);
        }
        finally
        {
            await fixture.DisposeAsync();
        }
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
