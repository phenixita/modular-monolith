using Xunit;

namespace VendingMachine.Orders.Tests.L1;

public sealed class InfrastructureFactAttribute : FactAttribute
{
    public InfrastructureFactAttribute()
    {
        var enabled = Environment.GetEnvironmentVariable("RUN_L1_TESTS");
        if (!string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set RUN_L1_TESTS=true to run infrastructure integration tests.";
        }
    }
}
