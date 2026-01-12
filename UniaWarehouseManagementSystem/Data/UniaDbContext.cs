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
        public DbSet<User> Users { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }

        public DbSet<Contractor> Contractors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=localhost;Database=UniaDb;User=root;Password=;";
            optionsBuilder.UseSqlite("Data Source=UniaWarehouse.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // To naprawia Twój b³¹d SQLite Error 1:
            // Tworzymy unikalny indeks na parze (WarehouseId + ProductId).
            // Dziêki temu baza wie, ¿e nie mo¿e byæ dwóch takich samych wpisów
            // i zadzia³a klauzula ON CONFLICT.
            modelBuilder.Entity<StockLevel>()
                .HasIndex(sl => new { sl.WarehouseId, sl.ProductId })
                .IsUnique();
        }

    }
}