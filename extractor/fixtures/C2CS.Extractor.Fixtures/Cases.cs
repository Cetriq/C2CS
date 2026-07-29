// The ten PoC fixture cases. Each demonstrates one resolution behavior; tests assert
// the extractor's exact response. Cases 2, 7, and 10 MUST stay unresolved — the
// extractor never guesses.
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;

namespace C2CS.Extractor.Fixtures
{
    public static class Case01ConstantProcessStart
    {
        public static void Run() => Process.Start("/usr/bin/git");
    }

    public static class Case02DynamicProcessStart
    {
        public static void Run(string toolPath) => Process.Start(toolPath);
    }

    public static class Case03ConstantEnvironmentVariable
    {
        public static string? Run() => Environment.GetEnvironmentVariable("DB_CONNECTION");
    }

    public static class Case04ConstantFilePath
    {
        public static void Run() => File.AppendAllText("/var/log/fixture.log", "entry");
    }

    public static class Case05ConcatenatedFilePath
    {
        public static void Run() => File.WriteAllText(string.Concat("/var/log/", "app.log"), "x");
    }

    public static class Case06ConstantHttpUri
    {
        public static Task Run(HttpClient client) => client.GetAsync("https://api.example.test/v1/status");
    }

    public static class Case07UriFromConfiguration
    {
        public static Task Run(HttpClient client, string uri) => client.GetAsync(uri);
    }

    public static class Case08TcpClient
    {
        public static void Run()
        {
            using var client = new TcpClient("db.example.test", 5432);
        }
    }

    public static class Case09ConnectionString
    {
        public static void Run() => _ = new FakeSql.SqlConnection("Server=sql.example.test,1433;Database=fixtures;Encrypt=true");
    }

    public static class Case10WrapperMethod
    {
        public static void Run() => StartTool("/usr/bin/tool");

        private static void StartTool(string executable) => Process.Start(executable);
    }
}

namespace FakeSql
{
    /// <summary>Stand-in for a SqlClient connection so fixtures avoid the package dependency.</summary>
    public sealed class SqlConnection
    {
        public SqlConnection(string connectionString) => _ = connectionString;
    }
}
