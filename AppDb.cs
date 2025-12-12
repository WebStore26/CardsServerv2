using CardsServer;
using Microsoft.EntityFrameworkCore;
// ===== EF Core =====

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> opts) : base(opts) { }
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Enterence> Enterence => Set<Enterence>();

}
