namespace VendingMachine.Persistence;

public sealed class InfrastructureOptions
{
    public const string SectionName = "postgres";

    public string ConnectionString { get; set; } = string.Empty;
}
