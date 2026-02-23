using Microsoft.Extensions.Options;

namespace VendingMachine.Persistence;

public sealed class InfrastructureOptionsValidator : IValidateOptions<InfrastructureOptions>
{
    private const string MissingConnectionStringMessage = "Missing configuration value 'postgres.connectionString'.";

    public ValidateOptionsResult Validate(string? name, InfrastructureOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(MissingConnectionStringMessage);
        }

        return ValidateOptionsResult.Success;
    }
}
