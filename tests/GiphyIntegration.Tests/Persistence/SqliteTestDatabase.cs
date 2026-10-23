using GiphyIntegration.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GiphyIntegration.Tests.Persistence;

/// <summary>
/// An in-memory SQLite database built from the real migrations, so tests also cover the schema.
/// It lives as long as its connection stays open.
/// </summary>
internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();
        using var dbContext = CreateDbContext();
        dbContext.Database.Migrate();
    }

    public GifCacheDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<GifCacheDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
