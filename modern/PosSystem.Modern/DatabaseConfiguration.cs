using Microsoft.Data.SqlClient;
using System.Xml.Linq;

namespace BikeZonePOS.Modern;

internal sealed class DatabaseConfiguration
{
    private const string ConnectionName = "Pos_System.Properties.Settings.pos_systemConnectionString";

    public string ConnectionString { get; }

    public DatabaseConfiguration()
    {
        ConnectionString = Resolve();
    }

    private static string Resolve()
    {
        string? environment = Environment.GetEnvironmentVariable("BIKEZONEPOS_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environment)) return Validate(environment);

        foreach (string path in CandidatePaths())
        {
            if (!File.Exists(path)) continue;
            XDocument document = XDocument.Load(path);
            XElement? setting = document.Root?.Elements("add")
                .FirstOrDefault(e => string.Equals((string?)e.Attribute("name"), ConnectionName, StringComparison.OrdinalIgnoreCase));
            string? value = (string?)setting?.Attribute("connectionString");
            if (!string.IsNullOrWhiteSpace(value)) return Validate(value);
        }

        return string.Empty;
    }

    private static IEnumerable<string> CandidatePaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "Database.config");
        yield return @"C:\BikeZonePOS\App\Database.config";
    }

    private static string Validate(string connectionString)
    {
        SqlConnectionStringBuilder builder = new(connectionString)
        {
            TrustServerCertificate = true,
            ConnectTimeout = Math.Clamp(new SqlConnectionStringBuilder(connectionString).ConnectTimeout, 3, 15)
        };
        return builder.ConnectionString;
    }
}
