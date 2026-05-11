namespace Duotify.Membership.Web.IntegrationTests;

public sealed class SchemaContractTests
{
    [Fact]
    public void SchemaScript_DefinesRequiredTablesAndIndexes()
    {
        var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Duotify.Membership.Web"));
        var schemaPath = Path.Combine(rootPath, "Infrastructure", "Data", "Sql", "Schema", "001_member_registration.sql");
        var script = File.ReadAllText(schemaPath);

        Assert.Contains("CREATE TABLE dbo.Members", script);
        Assert.Contains("CREATE TABLE dbo.EmailVerificationChallenges", script);
        Assert.Contains("CREATE TABLE dbo.SecurityAuditLogs", script);
        Assert.Contains("UX_Members_Email", script);
        Assert.Contains("UX_Members_NationalId", script);
        Assert.Contains("UX_EmailVerificationChallenges_CurrentActive", script);
    }
}