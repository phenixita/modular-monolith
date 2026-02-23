internal sealed record CliConfiguration(CliPostgresConfig Postgres);

internal sealed record CliPostgresConfig(string ConnectionString);
