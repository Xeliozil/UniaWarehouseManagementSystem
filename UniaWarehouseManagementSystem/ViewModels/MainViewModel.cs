using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;
using UniaWarehouseManagementSystem.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.IO;
using System.Text;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // --- 1. DANE ---

        [ObservableProperty]
        private ObservableCollection<Product> _productsList;

        // Cache do wyszukiwarki
        private List<Product> _allProductsCache = new();

        [ObservableProperty]
        private Product _selectedProduct;

        public Action RequestLogoutAction { get; set; }

        [ObservableProperty]
        private string _statusMessage = "Gotowy";

        [ObservableProperty]
        private string _searchText = "";

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        public Visibility AdminVisibility =>
            AuthService.CurrentUser?.Role == "Admin" ? Visibility.Visible : Visibility.Collapsed;

        public string CurrentUserInfo =>
            $"Zalogowany: {AuthService.CurrentUser?.Username} ({AuthService.CurrentUser?.Role})";

        // --- 2. KONSTRUKTOR ---
        public MainViewModel()
        {
            ProductsList = new ObservableCollection<Product>();
            // Wywo³ujemy metodê LoadData (bez czekania w konstruktorze)
            _ = LoadData();
        }

        // --- 3. METODY POMOCNICZE ---

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ProductsList.Clear();
                foreach (var item in _allProductsCache) ProductsList.Add(item);
                return;
            }

            var lowerQuery = SearchText.ToLower();
            var filtered = _allProductsCache
                .Where(p => p.Name.ToLower().Contains(lowerQuery) || p.Code.ToLower().Contains(lowerQuery))
                .ToList();

            ProductsList.Clear();
            foreach (var item in filtered) ProductsList.Add(item);
        }

        // --- JEDYNA I G£ÓWNA METODA £ADOWANIA DANYCH ---
        [RelayCommand]
        private async Task LoadData()
        {
            StatusMessage = "£adowanie danych...";
            try
            {
                using (var context = new UniaDbContext())
                {
                    // 1. Pobieramy produkty
                    var products = await context.Products.ToListAsync();

                    // 2. Pobieramy historiê ruchów (Dok³adnie tak jak w Dashboardzie)
                    // U¿ywamy AsNoTracking dla wydajnoœci
                    var items = await context.DocumentItems
                                             .Include(i => i.Document)
                                             .AsNoTracking()
                                             .ToListAsync();

                    // 3. Obliczamy stan dla ka¿dego produktu (PZ - WZ)
                    foreach (var p in products)
                    {
                        // Przychody (PZ, PW, MM)
                        var income = items
                            .Where(i => i.ProductId == p.Id &&
                                       (i.Document.DocType == "PZ" || i.Document.DocType == "PW" || i.Document.DocType == "MM"))
                            .Sum(i => i.Quantity);

                        // Rozchody (WZ, RW)
                        var outcome = items
                            .Where(i => i.ProductId == p.Id &&
                                       (i.Document.DocType == "WZ" || i.Document.DocType == "RW"))
                            .Sum(i => i.Quantity);

                        // Wpisujemy wyliczony stan do obiektu (to zobaczy tabela)
                        p.TotalQuantity = income - outcome;
                    }

                    // 4. Aktualizujemy cache (wa¿ne dla wyszukiwarki!)
                    _allProductsCache.Clear();
                    _allProductsCache.AddRange(products);
                }

                // Odœwie¿amy widok
                ApplyFilter();
                StatusMessage = $"Za³adowano {_allProductsCache.Count} produktów.";
            }
            catch (Exception ex)
            {
                StatusMessage = "B³¹d: " + ex.Message;
                MessageBox.Show("B³¹d bazy danych: " + ex.Message);
            }
        }

        // --- 4. KOMENDY (PRZYCISKI) ---

        [RelayCommand]
        private void OpenAddProduct()
        {
            var editorVm = new ProductEditorViewModel();
            var window = new ProductWindow();
            editorVm.CloseAction = () => window.Close();
            window.DataContext = editorVm;
            window.ShowDialog();
            _ = LoadData();
        }

        [RelayCommand]
        private void OpenEditProduct()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Zaznacz produkt, który chcesz edytowaæ.");
                return;
            }
            var editorVm = new ProductEditorViewModel();
            var window = new ProductWindow();
            editorVm.LoadProduct(SelectedProduct);
            editorVm.CloseAction = () => window.Close();
            window.DataContext = editorVm;
            window.ShowDialog();
            _ = LoadData();
        }

        [RelayCommand]
        private void OpenNewPz()
        {
            var win = new DocumentWindow();
            if (win.DataContext is DocumentEditorViewModel vm) vm.SetMode("PZ");
            win.ShowDialog();
            _ = LoadData();
        }

        [RelayCommand]
        private void OpenNewWz()
        {
            var win = new DocumentWindow();
            if (win.DataContext is DocumentEditorViewModel vm) vm.SetMode("WZ");
            win.ShowDialog();
            _ = LoadData();
        }

        [RelayCommand]
        private void OpenNewMm()
        {
            var win = new DocumentWindow();
            if (win.DataContext is DocumentEditorViewModel vm)
            {
                vm.SetMode("MM");
                win.Title = "Przesuniêcie MM";
            }
            win.ShowDialog();
            _ = LoadData();
        }

        [RelayCommand]
        private void OpenNewContractor()
        {
            var vm = new ContractorEditorViewModel();
            var win = new ContractorWindow();
            vm.CloseAction = () => win.Close();
            win.DataContext = vm;
            win.ShowDialog();
        }

        [RelayCommand]
        private void GenerateReport()
        {
            try
            {
                if (ProductsList == null || ProductsList.Count == 0)
                {
                    MessageBox.Show("Brak danych do wydruku!");
                    return;
                }
                StatusMessage = "Generowanie PDF...";
                var generator = new PdfGenerator();
                generator.GenerateStockReport(ProductsList.ToList());
                StatusMessage = "Raport wygenerowany.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"B³¹d generowania PDF: {ex.Message}");
                StatusMessage = "B³¹d wydruku.";
            }
        }

        [RelayCommand]
        private void OpenHistory() => new HistoryWindow().ShowDialog();

        [RelayCommand]
        private void Logout()
        {
            AuthService.Logout();
            RequestLogoutAction?.Invoke();
        }

        [RelayCommand]
        private void OpenNewUser()
        {
            var vm = new UserEditorViewModel();
            var win = new UserWindow();
            vm.CloseAction = () => win.Close();
            win.DataContext = vm;
            win.ShowDialog();
        }

        [RelayCommand]
        private void ExitApp()
        {
            AuthService.Logout();
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private void OpenSettings() => new SettingsWindow().ShowDialog();

        [RelayCommand]
        private void OpenStats() => new StatsWindow().ShowDialog();

        [RelayCommand]
        private async Task ImportProducts()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pliki CSV (*.csv)|*.csv|Wszystkie pliki (*.*)|*.*",
                Title = "Wybierz plik z produktami (Format: Kod;Nazwa;Jm;MinStan)"
            };

            if (dialog.ShowDialog() != true) return;

            string filePath = dialog.FileName;
            int added = 0;
            int updated = 0;
            int errors = 0;

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                using (var context = new UniaDbContext())
                {
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.StartsWith("Kod", StringComparison.OrdinalIgnoreCase)) continue;

                        var parts = line.Split(';');
                        if (parts.Length < 3) { errors++; continue; }

                        string code = parts[0].Trim();
                        string name = parts[1].Trim();
                        string unit = parts[2].Trim();
                        decimal minStock = 0;
                        if (parts.Length > 3 && decimal.TryParse(parts[3].Trim(), out decimal m)) minStock = m;

                        var existingProduct = await context.Products.FirstOrDefaultAsync(p => p.Code == code);

                        if (existingProduct == null)
                        {
                            var newProd = new Product
                            {
                                Code = code,
                                Name = name,
                                Unit = unit,
                                MinStock = minStock,
                                TotalQuantity = 0
                            };
                            context.Products.Add(newProd);
                            added++;
                        }
                        else
                        {
                            existingProduct.Name = name;
                            existingProduct.Unit = unit;
                            existingProduct.MinStock = minStock;
                            updated++;
                        }
                    }
                    await context.SaveChangesAsync();
                }

                // Tutaj wo³amy nasz¹ naprawion¹ metodê LoadData
                await LoadData();

                MessageBox.Show($"Zakoñczono import!\n\nDodano: {added}\nZaktualizowano: {updated}\nB³êdy: {errors}",
                                "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"B³¹d importu: {ex.Message}", "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}