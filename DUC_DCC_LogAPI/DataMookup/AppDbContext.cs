using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.DataMookup;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<History> Histories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<History>(entity =>
        {
            entity.ToTable("History");

            entity.Property(e => e.action).HasMaxLength(50);
            entity.Property(e => e.action_datetime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.admin_confirm_evnet).HasMaxLength(50);
            entity.Property(e => e.app_log).HasMaxLength(20);
            entity.Property(e => e.bu_code).HasMaxLength(50);
            entity.Property(e => e.emp_no).HasMaxLength(50);
            entity.Property(e => e.event_type).HasMaxLength(50);
            entity.Property(e => e.fullname).HasMaxLength(150);
            entity.Property(e => e.group_name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.processType).HasMaxLength(50);
            entity.Property(e => e.username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
