using DUC_DCC_LogAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DUC_DCC_LogAPI.Data
{
    public class AppDbContext : DbContext
    { 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
         
        public DbSet<DUC_DCC_Log> DUC_DCC_Log { get; set; }
        public DbSet<Users_Permission> Users_Permission { get; set; }
        public DbSet<Dcc_crud> Dcc_cruds { get; set; }
    }
}
