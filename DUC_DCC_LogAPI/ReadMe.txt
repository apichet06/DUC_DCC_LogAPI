dotnet ef dbcontext scaffold "Server=192.168.99.23;Database=bmsdb;User Id=bmsdba;Password=SDb@45#99" Npgsql.EntityFrameworkCore.PostgreSQL -o DataMookup -c BMTADbContext -t UserGroup --no-onconfiguring -f

dotnet ef dbcontext scaffold "Server=10.3.2.44,14000;Initial Catalog=dbCRUD;User ID=FITS_RW;Password=fits$testin9;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o DataMookup -c AppDbContext -t DUC_DCC_Log --no-onconfiguring -f


dotnet ef dbcontext scaffold "Server=Terng\SQLEXPRESS;Initial Catalog=dbCRUD;User ID=sa;Password=123456;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o DataMookup -c AppDbContext -t History --no-onconfiguring --use-database-names -f

