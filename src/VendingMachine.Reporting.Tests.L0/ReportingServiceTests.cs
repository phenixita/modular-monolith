using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using VendingMachine.Orders;
using VendingMachine.Reporting.Abstractions;
using Xunit;

namespace VendingMachine.Reporting.Tests.L0;

public sealed class ReportingServiceTests
{
    [Fact]
    public async Task GetDashboardStats_WhenCalled_ReturnsRepositoryStats()
    {
        var repository = Substitute.For<IReportingRepository>();
        var expected = new DashboardStats(12.50m, 5, 2.50m);
        repository.GetDashboardStatsAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingModuleForTesting(repository);
        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IReportingService>();
        var actual = await service.GetDashboardStatsAsync();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task PublishOrderConfirmed_WhenCalled_RecordsOrderInReportingRepository()
    {
        var repository = Substitute.For<IReportingRepository>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddReportingModuleForTesting(repository);
        var provider = services.BuildServiceProvider();

        var publisher = provider.GetRequiredService<IPublisher>();
        var occurredAt = new DateTimeOffset(2026, 2, 25, 12, 0, 0, TimeSpan.Zero);

        await publisher.Publish(new OrderConfirmed("COLA", 1.50m, occurredAt));

        await repository.Received(1)
            .RecordOrderAsync("COLA", 1.50m, occurredAt, Arg.Any<CancellationToken>());
    }
}
