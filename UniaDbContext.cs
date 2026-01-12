using Microsoft.EntityFrameworkCore;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.Data
{
    public class UniaDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Konfiguracja pod XAMPP (domyœlna):
            // User=root
            // Password= (puste)
            var connectionString = "Server=localhost;Database=UniaDb;User=root;Password=;";
            
            // Wersja 7.0 Pomelo wymaga okreœlenia wersji serwera przy konfiguracji
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}