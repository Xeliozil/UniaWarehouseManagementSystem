using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class StatsViewModel : ObservableObject
    {
        // --- 1. KAFELKI ---
        [ObservableProperty] private int _lowStockCount;
        [ObservableProperty] private int _totalProductsCount;
        [ObservableProperty] private decimal _totalStockSum;
        [ObservableProperty] private int _docsThisMonth;

        // --- 2. LISTA BRAKÓW ---
        [ObservableProperty] private ObservableCollection<Product> _lowStockProducts;

        public StatsViewModel()
        {
            LowStockProducts = new ObservableCollection<Product>();
            CalculateStats();
        }

        private async void CalculateStats()
        {
            using (var context = new UniaDbContext())
            {
                // 1. Pobieramy produkty i historię ruchów (DocumentItems)
                // Używamy AsNoTracking dla szybkości
                var allProducts = await context.Products.AsNoTracking().ToListAsync();
                var allItems = await context.DocumentItems
                                            .Include(i => i.Document)
                                            .AsNoTracking()
                                            .ToListAsync();

                // 2. PRZELICZAMY PRAWDZIWY STAN (PZ - WZ)
                foreach (var p in allProducts)
                {
                    // Suma przychodów (PZ, PW, MM+)
                    var income = allItems
                        .Where(i => i.ProductId == p.Id &&
                                   (i.Document.DocType == "PZ" || i.Document.DocType == "PW" || i.Document.DocType == "MM"))
                        .Sum(i => i.Quantity);

                    // Suma rozchodów (WZ, RW)
                    // Uwaga: MM traktujemy specyficznie, tu dla uproszczenia zakładam, że MM to przesunięcie wewnętrzne, 
                    // ale jeśli MM zmniejsza stan magazynu źródłowego, trzeba by to uwzględnić. 
                    // W prostym modelu PZ/WZ skupmy się na nich:
                    var outcome = allItems
                        .Where(i => i.ProductId == p.Id &&
                                   (i.Document.DocType == "WZ" || i.Document.DocType == "RW"))
                        .Sum(i => i.Quantity);

                    // Nadpisujemy stan w pamięci (to naprawi widok)
                    p.TotalQuantity = income - outcome;
                }

                // 3. Obliczamy statystyki na podstawie przeliczonych danych
                TotalProductsCount = allProducts.Count;
                TotalStockSum = allProducts.Sum(p => p.TotalQuantity);

                // Alarmy: Tylko tam, gdzie stan jest mniejszy od minimum (i minimum jest ustawione > 0)
                // Dodaliśmy warunek p.MinStock > 0, żeby nie pokazywało śmieci, jeśli ktoś nie ustawił minimum.
                var alerts = allProducts
                    .Where(p => p.MinStock > 0 && p.TotalQuantity < p.MinStock)
                    .ToList();

                LowStockCount = alerts.Count;

                LowStockProducts.Clear();
                foreach (var p in alerts) LowStockProducts.Add(p);

                // 4. Dokumenty z tego miesiąca
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DocsThisMonth = await context.Documents
                    .Where(d => d.DocDate >= startOfMonth)
                    .CountAsync();
            }
        }
    }
}