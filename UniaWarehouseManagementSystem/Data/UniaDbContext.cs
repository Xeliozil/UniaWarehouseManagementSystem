using Microsoft.EntityFrameworkCore;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.Data
{
    public class UniaDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<StockLevel> StockLevels { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentItem> DocumentItems { get; set; }

        // NOWE:
        public DbSet<Contractor> Contractors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=localhost;Database=UniaDb;User=root;Password=;";
            optionsBuilder.UseSqlite("Data Source=UniaWarehouse.db");
        }
        public DbSet<User> Users { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
    }
}