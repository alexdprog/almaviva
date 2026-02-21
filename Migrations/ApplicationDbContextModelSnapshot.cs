using System;
using AlmavivaSlotChecker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlmavivaSlotChecker.Migrations;

[DbContext(typeof(ApplicationDbContext))]
public partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("AlmavivaSlotChecker.Data.ApplicationUser", b =>
        {
            b.Property<string>("Id").HasColumnType("text");
            b.Property<int>("AccessFailedCount").HasColumnType("integer");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("text");
            b.Property<string>("Email").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<bool>("EmailConfirmed").HasColumnType("boolean");
            b.Property<bool>("LockoutEnabled").HasColumnType("boolean");
            b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("timestamp with time zone");
            b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("PasswordHash").HasColumnType("text");
            b.Property<string>("PhoneNumber").HasColumnType("text");
            b.Property<bool>("PhoneNumberConfirmed").HasColumnType("boolean");
            b.Property<string>("SecurityStamp").HasColumnType("text");
            b.Property<bool>("TwoFactorEnabled").HasColumnType("boolean");
            b.Property<string>("UserName").HasMaxLength(256).HasColumnType("character varying(256)");
            b.HasKey("Id");
            b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
            b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex");
            b.ToTable("AspNetUsers", (string)null);
        });

        modelBuilder.Entity("AlmavivaSlotChecker.Entities.SlotCheckLog", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            b.Property<DateTimeOffset>("CheckedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("ErrorMessage").HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.Property<bool>("IsAvailable").HasColumnType("boolean");
            b.Property<string>("RawResponse").IsRequired().HasColumnType("text");
            b.HasKey("Id");
            b.HasIndex("CheckedAt");
            b.ToTable("SlotCheckLogs");
        });

        modelBuilder.Entity("AlmavivaSlotChecker.Entities.SlotCheckSettings", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            b.Property<int>("CheckIntervalMinutes").ValueGeneratedOnAdd().HasColumnType("integer").HasDefaultValue(5);
            b.Property<string>("TelegramBotToken").IsRequired().HasMaxLength(300).HasColumnType("character varying(300)");
            b.Property<string>("TelegramChatId").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("Url").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.HasKey("Id");
            b.ToTable("SlotCheckSettings");
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
        {
            b.Property<string>("Id").HasColumnType("text");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("text");
            b.Property<string>("Name").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("character varying(256)");
            b.HasKey("Id");
            b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex");
            b.ToTable("AspNetRoles", (string)null);
        });
    }
}
