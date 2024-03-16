using Microsoft.EntityFrameworkCore;

namespace Sanguosha.Lobby.Server;

public class AccountContext(DbContextOptions<AccountContext> options) : DbContext(options)
{
    public DbSet<Core.Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Core.Account>()
            .HasKey(a => a.UserName);
        modelBuilder.Entity<Core.Account>()
            .Property(a => a.Password)
            .IsRequired();
    }
}
