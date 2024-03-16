using Microsoft.EntityFrameworkCore;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public class AccountContext(DbContextOptions<AccountContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .ToTable("Accounts", schema: "Auth");
        modelBuilder.Entity<Account>()
            .HasKey(a => a.UserName);
        modelBuilder.Entity<Account>()
            .Property(a => a.UserName)
            .HasColumnName("user_name");
        modelBuilder.Entity<Account>()
            .Property(a => a.Password)
            .IsRequired()
            .HasColumnName("password");
    }
}
