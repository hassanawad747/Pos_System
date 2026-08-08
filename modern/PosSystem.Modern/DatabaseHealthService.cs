using Microsoft.Data.SqlClient;

namespace BikeZonePOS.Modern;

internal sealed class DatabaseHealthService
{
    private readonly DatabaseConfiguration configuration;

    public DatabaseHealthService(DatabaseConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<DatabaseHealth> CheckAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
            return new DatabaseHealth(false, "Database.config was not found.", null, null);

        try
        {
            await using SqlConnection connection = new(configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using SqlCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT DB_NAME(), CAST(SERVERPROPERTY('ServerName') AS nvarchar(128)),
(SELECT COUNT(*) FROM dbo.SchemaMigrations);";
            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return new DatabaseHealth(false, "SQL returned no health information.", null, null);

            string database = reader.IsDBNull(0) ? "" : reader.GetString(0);
            string server = reader.IsDBNull(1) ? "" : reader.GetString(1);
            int migrations = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
            return new DatabaseHealth(true, $"Connected. {migrations} migrations applied.", server, database);
        }
        catch (Exception ex)
        {
            return new DatabaseHealth(false, ex.Message, null, null);
        }
    }
}

internal sealed record DatabaseHealth(bool IsConnected, string Message, string? Server, string? Database);
