using Microsoft.EntityFrameworkCore.Storage;

namespace Common.EfCore.DbContext;

public interface IDbContext //TODO extract to separate lib .Abstractions
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}