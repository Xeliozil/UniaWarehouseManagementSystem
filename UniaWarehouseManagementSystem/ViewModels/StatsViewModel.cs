using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class StatsViewModel : ObservableObject
    {
        [ObservableProperty] private int _lowStockCount;
        [ObservableProperty] private int _totalProductsCount;
        [ObservableProperty] private decimal _totalStockSum;
        [ObservableProperty] private int _docsThisMonth;

        [ObservableProperty] private ObservableCollection<Product> _lowStockProducts;

        public StatsViewModel()
        {
            LowStockProducts = new ObservableCollection<Product>();
            _ = CalculateStats(); // Uruchamiamy asynchronicznie
        }

        private async Task CalculateStats()
        {
            try
            {
                using (var context = new UniaDbContext())
                {
                    // 1. Pobieramy produkty WRAZ z ich stanami (Include)
                    // Nie potrzebujemy już pobierać allItems i liczyć ręcznie!
                    var allProducts = await context.Products
                        .Include(p => p.StockLevels)
                        .AsNoTracking()
                        .ToListAsync();

                    // 2. Obliczamy statystyki
                    // Właściwość TotalQuantity wyliczy się sama w modelu Product
                    TotalProductsCount = allProducts.Count;
                    TotalStockSum = allProducts.Sum(p => p.TotalQuantity);

                    // 3. Alarmy: Filtrujemy produkty z niskim stanem
                    var alerts = allProducts
                        .Where(p => p.MinStock > 0 && p.TotalQuantity < p.MinStock)
                        .ToList();

                    LowStockCount = alerts.Count;

                    // Odświeżamy listę braków na widoku
                    LowStockProducts.Clear();
                    foreach (var p in alerts) LowStockProducts.Add(p);

                    // 4. Dokumenty z tego miesiąca
                    var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    DocsThisMonth = await context.Documents
                        .Where(d => d.DocDate >= startOfMonth)
                        .CountAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Błąd statystyk: " + ex.Message);
            }
        }
    }
}