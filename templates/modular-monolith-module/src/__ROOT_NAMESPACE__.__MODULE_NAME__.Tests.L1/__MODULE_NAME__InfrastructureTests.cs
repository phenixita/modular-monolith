using Xunit;

namespace __ROOT_NAMESPACE__.__MODULE_NAME__.Tests.L1;

[Trait("Level", "L1")]
public sealed class __MODULE_NAME__InfrastructureTests
{
    [Fact]
    public void Bootstrap_ModuleProjects_AreReferenced()
    {
        var repository = new Infrastructure.InMemory__MODULE_NAME__Repository();

        Assert.NotNull(repository);
    }
}
