using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Simple IConfiguration wrapper for CLI configuration.
/// </summary>
internal sealed class CliConfigurationWrapper : IConfiguration
{
    private readonly Dictionary<string, string?> _values;

    public CliConfigurationWrapper(CliConfiguration config)
    {
        _values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["postgres:connectionString"] = config.Postgres.ConnectionString,
            ["mongo:connectionString"] = config.Mongo.ConnectionString,
            ["mongo:database"] = config.Mongo.Database
        };
    }

    public string? this[string key]
    {
        get => _values.TryGetValue(key, out var value) ? value : null;
        set => _values[key] = value;
    }

    public IConfigurationSection GetSection(string key) => throw new NotImplementedException();

    public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();

    public IChangeToken GetReloadToken() => throw new NotImplementedException();
}
