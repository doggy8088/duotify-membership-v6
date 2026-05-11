using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Duotify.Membership.Web.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureSchemaAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<SqlConnectionFactory>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (!environment.IsDevelopment())
        {
            return;
        }

        var schemaPath = Path.Combine(environment.ContentRootPath, "Infrastructure", "Data", "Sql", "Schema", "001_member_registration.sql");
        if (!File.Exists(schemaPath))
        {
            return;
        }

        var script = await File.ReadAllTextAsync(schemaPath, cancellationToken);
        var commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(static batch => !string.IsNullOrWhiteSpace(batch));

        await using var connection = factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        foreach (var commandText in commands)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}