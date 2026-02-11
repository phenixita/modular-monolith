internal sealed record CliConfiguration(CliPostgresConfig Postgres, CliMongoConfig Mongo);

internal sealed record CliPostgresConfig(string ConnectionString);

internal sealed record CliMongoConfig(string ConnectionString, string Database);
