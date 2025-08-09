using DUC_DCC_LogAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.Data
{
    public class AppDbContext : DbContext
    { 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
         
        public DbSet<Application_log> Application_Log { get; set; }
        public DbSet<Users_Permission> Users_Permission { get; set; }
        public DbSet<dcc_crud_log> dcc_crud_log { get; set; }
        public DbSet<Duc_crud> Duc_Cruds { get; set; }
        public DbSet<Month> Month { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.Entity<dcc_crud_log>().HasNoKey(); 
        
        }

    }
}
