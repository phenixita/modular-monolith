using System.Text.Json;

internal static class CliConfigurationLoader
{
    public static CliConfiguration Load(FileInfo configFile)
    {
        if (!configFile.Exists)
        {
            throw new InvalidOperationException(
                $"Config file not found at '{configFile.FullName}'. " +
                "Create it with: {\"postgres\":{\"connectionString\":\"Host=localhost;Port=5432;Database=vendingmachine;Username=postgres;Password=postgres\"}}");
        }

        var json = File.ReadAllText(configFile.FullName);
        var config = JsonSerializer.Deserialize<CliConfiguration>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Postgres is null)
        {
            throw new InvalidOperationException("Invalid config file. 'postgres' section is required.");
        }

        if (string.IsNullOrWhiteSpace(config.Postgres.ConnectionString))
        {
            throw new InvalidOperationException("Invalid config file. 'postgres.connectionString' is required.");
        }

        return config;
    }
}
