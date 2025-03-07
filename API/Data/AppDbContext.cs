using Microsoft.EntityFrameworkCore;

namespace API.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Account> Accounts => Set<Account>();
    //public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var transaction = modelBuilder.Entity<Transaction>();
        transaction.Property(t => t.Id).ValueGeneratedOnAdd();
        transaction.Property(t => t.RowVersion).HasDefaultValue(0).IsRowVersion();
        transaction.Property(t => t.CreatedAt).HasDefaultValueSql("datetime('now')");

        var account = modelBuilder.Entity<Account>();
        account.Property(t => t.RowVersion).HasDefaultValue(0).IsRowVersion();
        account.Property(a => a.Id).ValueGeneratedOnAdd();

        //var user = modelBuilder.Entity<User>();
        //user.Property(u => u.Id).ValueGeneratedOnAdd();
    }
}
