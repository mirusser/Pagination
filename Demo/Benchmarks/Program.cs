
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Npgsql;

BenchmarkRunner.Run<Benchmarks>();

// Benchmark for more low level case
// without use of EF Core
// run in release mode: `dotnet run -c Release`
public class Benchmarks
{
    private const string ConnectionString =
        "Host=your_host;Username=your_user;Password=super_secret_password;Database=demo";
    private NpgsqlConnection _connection;
    
    [Params(20, 900000)]
    public int Value { get; set; }
    
    [GlobalSetup]
    public  async Task Setup()
    {
        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();
    }

    [Benchmark]
    public async Task Offset_Pagination_ById()
    {
        await using var command = new NpgsqlCommand(
            $"""
                SELECT b."Id", b."LastUpdated"
                FROM "Blogs" AS b
                ORDER BY b."Id"
                LIMIT 10 OFFSET {Value}
                """, _connection);
        await command.ExecuteNonQueryAsync();
    }
    
    [Benchmark]
    public async Task Keyset_Pagination_ById()
    {
        await using var command = new NpgsqlCommand(
            $"""
             SELECT b."Id", b."LastUpdated"
             FROM "Blogs" AS b
             WHERE b."Id" > {Value}
             ORDER BY b."Id"
             """, _connection);
        await command.ExecuteNonQueryAsync();
    }
    
    [GlobalCleanup]
    public void Cleanup()
        => _connection?.Dispose();
}