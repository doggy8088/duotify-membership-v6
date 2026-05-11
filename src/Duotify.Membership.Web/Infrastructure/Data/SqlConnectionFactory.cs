using Microsoft.Data.SqlClient;

namespace Duotify.Membership.Web.Infrastructure.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

    public SqlConnection CreateConnection() => new(_connectionString);
}