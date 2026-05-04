using Auth.Core.Entities;
using Common.Outbox;
using Common.StronglyTypedIds.StronglyTypedIds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Auth.Core.Data;

public sealed class AuthDbContext : IdentityDbContext<User, Role, AuthId>
{
    public const string Schema = "Auth";

    public DbSet<OutboxMessage> OutboxMessages { get; init; }

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var authIdConverter = new ValueConverter<AuthId, Guid>(
            id => id.Value,
            value => new AuthId(value));

        modelBuilder.HasDefaultSchema(Schema);

        base.OnModelCreating(modelBuilder);
        modelBuilder.UseOpenIddict();
        modelBuilder.ConfigureOutbox(Schema);

        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(e => e.Id).HasConversion(authIdConverter);
        });

        modelBuilder.Entity<Role>(builder =>
        {
            builder.Property(e => e.Id).HasConversion(authIdConverter);
        });

        modelBuilder.Entity<IdentityUserClaim<AuthId>>(builder =>
        {
            builder.Property(e => e.UserId).HasConversion(authIdConverter);
        });

        modelBuilder.Entity<IdentityUserLogin<AuthId>>(builder =>
        {
            builder.Property(e => e.UserId).HasConversion(authIdConverter);
        });

        modelBuilder.Entity<IdentityUserToken<AuthId>>(builder =>
        {
            builder.Property(e => e.UserId).HasConversion(authIdConverter);
        });

        modelBuilder.Entity<IdentityRoleClaim<AuthId>>(builder =>
        {
            builder.Property(e => e.RoleId).HasConversion(authIdConverter);
        });

        modelBuilder.Entity<IdentityUserRole<AuthId>>(builder =>
        {
            builder.Property(e => e.UserId).HasConversion(authIdConverter);
            builder.Property(e => e.RoleId).HasConversion(authIdConverter);
        }); ;
    }
}
