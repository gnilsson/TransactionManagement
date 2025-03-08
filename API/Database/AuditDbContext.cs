using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Database;

public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var auditLog = modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(a => a.Id).ValueGeneratedOnAdd();
            entity.Property(a => a.Timestamp).HasDefaultValueSql("datetime('now')");
        });

        modelBuilder.HasDefaultSchema("audit");
    }
}
