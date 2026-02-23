namespace VendingMachine.;

internal sealed class InfrastructureOptions
{
    public const string SectionName = "postgres";

    public string ConnectionString { get; set; } = string.Empty;
}
