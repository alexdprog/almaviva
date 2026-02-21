using AlmavivaSlotChecker.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlmavivaSlotChecker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<SlotCheckSettings> SlotCheckSettings => Set<SlotCheckSettings>();
    public DbSet<SlotCheckLog> SlotCheckLogs => Set<SlotCheckLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SlotCheckSettings>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Url).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.TelegramBotToken).HasMaxLength(300);
            entity.Property(x => x.TelegramChatId).HasMaxLength(100);
            entity.Property(x => x.CheckIntervalMinutes).HasDefaultValue(5);
        });

        builder.Entity<SlotCheckLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RawResponse).HasColumnType("text");
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(x => x.CheckedAt);
        });
    }
}
