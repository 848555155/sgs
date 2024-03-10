using Microsoft.EntityFrameworkCore;

namespace Sanguosha.Lobby.Server;

public class AccountContext(DbContextOptions<AccountContext> options) : DbContext(options)
{
    // todo move to appsettings.json
    //@"data source=(LocalDB)\v11.0; 
    //     initial catalog=users;
    //     integrated security=true"

    public DbSet<Core.Account> Accounts { get; set; }
}
