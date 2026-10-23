using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GiphyIntegration.Infrastructure.Persistence;

internal static class DbUpdateExceptionExtensions
{
    // SQLITE_CONSTRAINT_UNIQUE, see https://www.sqlite.org/rescode.html#constraint_unique
    private const int SqliteUniqueConstraintViolation = 2067;

    /// <remarks>Provider specific: adapt when switching database (e.g. SQL Server errors 2601/2627).</remarks>
    public static bool IsUniqueConstraintViolation(this DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteUniqueConstraintViolation };
}
