using AlmavivaSlotChecker.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlmavivaSlotChecker.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserLoginAudit> UserLoginAudits => Set<UserLoginAudit>();
    public DbSet<SlotCheckAudit> SlotCheckAudits => Set<SlotCheckAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLoginAudit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
        });

        modelBuilder.Entity<SlotCheckAudit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VisaCenterCode).HasMaxLength(32);
            entity.Property(x => x.ServiceCode).HasMaxLength(64);
            entity.Property(x => x.RawResponse).HasMaxLength(4000);
        });
    }
}
