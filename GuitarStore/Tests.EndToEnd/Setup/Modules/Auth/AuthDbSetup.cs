using Auth.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.EndToEnd.Setup.Modules.Common;

namespace Tests.EndToEnd.Setup.Modules.Auth;

internal sealed class AuthDbSetup : IDbSetup
{
    public void SetupDb(IServiceCollection services, string connectionString)
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(connectionString)
            .EnableDetailedErrors();

        var context = new AuthDbContext(dbOptionsBuilder.Options);

        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        services.RemoveAll<AuthDbContext>();
        services.RemoveAll<DbContextOptions<AuthDbContext>>();
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseOpenIddict();
        });
    }
}
