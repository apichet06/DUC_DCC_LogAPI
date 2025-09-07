 
using DUC_DCC_LogAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.Data
{
    public partial class AppDbContext : DbContext
    { 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
         
        public DbSet<Application_log> Application_Log { get; set; }
        public DbSet<Users_Permission> Users_Permission { get; set; }
        public DbSet<dcc_crud_log> dcc_crud_log { get; set; }
        public DbSet<Duc_crud> Duc_Cruds { get; set; }
        public DbSet<Month> Month { get; set; }
        public DbSet<App_Name> app_name { get; set; }
        public DbSet<Bu_Plant> bu_plant { get; set; }
        public DbSet<Historys> history { get; set; }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder); 
        //    modelBuilder.Entity<dcc_crud_log>().HasNoKey(); 

        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<dcc_crud_log>().HasNoKey();
            modelBuilder.Entity<Historys>(entity =>
            {
                entity.ToTable("History");

                entity.Property(e => e.action).HasMaxLength(50);
                entity.Property(e => e.action_datetime)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.admin_confirm_event).HasMaxLength(50);
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
}
