using System.Data.Entity;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

internal class AccountContext : DbContext
{
    public AccountContext()
        : base(@"data source=(LocalDB)\v11.0; 
                 initial catalog=users;
                 integrated security=true")
    {
    }
    public DbSet<Account> Accounts { get; set; }
}
