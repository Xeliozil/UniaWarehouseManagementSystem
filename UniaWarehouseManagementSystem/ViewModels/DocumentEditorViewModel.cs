using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class DocumentEditorViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Warehouse> _warehouses;
        [ObservableProperty] private ObservableCollection<Product> _products;

        // --- ZMIANA: Dwa osobne magazyny ---
        [ObservableProperty] private Warehouse _sourceWarehouse; // Skąd (Dla WZ i MM)
        [ObservableProperty] private Warehouse _targetWarehouse; // Dokąd (Dla PZ i MM)

        [ObservableProperty] private string _documentNumber;
        [ObservableProperty] private string _currentDocType = "PZ";

        // --- ZMIANA: Sterowanie widocznością w oknie ---
        [ObservableProperty] private Visibility _showSource = Visibility.Collapsed;
        [ObservableProperty] private Visibility _showTarget = Visibility.Visible;

        [ObservableProperty] private string _stockInfo = "";
        [ObservableProperty] private decimal _maxAvailable = 0;

        [ObservableProperty] private Product _currentProduct;
        [ObservableProperty] private string _currentQuantity = "1";
        [ObservableProperty] private ObservableCollection<DocumentItem> _cartItems;

        public Action CloseAction { get; set; }

        public DocumentEditorViewModel()
        {
            Warehouses = new ObservableCollection<Warehouse>();
            Products = new ObservableCollection<Product>();
            CartItems = new ObservableCollection<DocumentItem>();
            FilteredContractors = new ObservableCollection<Contractor>();
            DocumentNumber = "AUTO";
            LoadDictionariesAsync();
        }

        public void SetMode(string docType)
        {
            CurrentDocType = docType;
            DocumentNumber = $"{docType}/{DateTime.Now:yyMMdd}/{DateTime.Now.Ticks % 10000}";

            // Konfiguracja widoczności pól w zależności od typu
            if (docType == "PZ")
            {
                ShowSource = Visibility.Collapsed;
                ShowTarget = Visibility.Visible;
            }
            else if (docType == "WZ")
            {
                ShowSource = Visibility.Visible;
                ShowTarget = Visibility.Collapsed;
            }
            else if (docType == "MM")
            {
                ShowSource = Visibility.Visible;
                ShowTarget = Visibility.Visible;
            }

            FilterContractors();
        }

        // Gdy zmienimy magazyn źródłowy lub produkt -> sprawdzamy stan
        partial void OnSourceWarehouseChanged(Warehouse value) => CheckStock();
        partial void OnCurrentProductChanged(Product value) => CheckStock();

        private void CheckStock()
        {
            if (CurrentProduct == null)
            {
                StockInfo = ""; return;
            }

            // Stan sprawdzamy tylko w magazynie ŹRÓDŁOWYM (dla WZ i MM)
            // Dla PZ stan nas nie obchodzi przy wprowadzaniu (chyba że informacyjnie)
            if (CurrentDocType == "PZ")
            {
                StockInfo = "Przyjęcie towaru";
                return;
            }

            if (SourceWarehouse == null) return;

            using (var ctx = new UniaDbContext())
            {
                var stock = ctx.StockLevels
                    .FirstOrDefault(s => s.WarehouseId == SourceWarehouse.Id && s.ProductId == CurrentProduct.Id);

                decimal qty = stock?.Quantity ?? 0;
                MaxAvailable = qty;
                StockInfo = $"Dostępne w źródle: {qty} {CurrentProduct.Unit}";
            }
        }

        private async Task LoadDictionariesAsync()
        {
            using (var context = new UniaDbContext())
            {
                var wList = await context.Warehouses.ToListAsync();
                var pList = await context.Products.ToListAsync();
                // Pobieramy kontrahentów
                var cList = await context.Contractors.ToListAsync();

                Warehouses = new ObservableCollection<Warehouse>(wList);
                Products = new ObservableCollection<Product>(pList);

                _allContractors = cList; // Zapisz do cache

                // Domyślne ustawienia
                TargetWarehouse = Warehouses.FirstOrDefault();
                SourceWarehouse = Warehouses.FirstOrDefault();

                // Wymuś odświeżenie listy kontrahentów
                FilterContractors();
                // Wewnątrz bloku using (var context ...), przy tworzeniu new Document:
                if ((CurrentDocType == "PZ" || CurrentDocType == "MM") && TargetWarehouse == null)
                {
                    // Tutaj wyświetl komunikat dla użytkownika, np. MessageBox
                    MessageBox.Show("Musisz wybrać magazyn docelowy dla dokumentu PZ!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Przerwij zapisywanie
                }

                var newDoc = new Document
                {
                    Number = DocumentNumber,
                    DocType = CurrentDocType,
                    DocDate = DateTime.Now,
                    // ... reszta kodu
                    TargetWarehouseId = (CurrentDocType == "PZ" || CurrentDocType == "MM") ? TargetWarehouse.Id : null,
                    // ...
                };
            }
        }

        [RelayCommand]
        private void AddToCart()
        {
            if (CurrentProduct == null) return;
            if (!decimal.TryParse(CurrentQuantity, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Podaj poprawną ilość!"); return;
            }

            // Walidacja stanu dla WZ i MM
            if ((CurrentDocType == "WZ" || CurrentDocType == "MM") && qty > MaxAvailable)
            {
                MessageBox.Show($"Brak towaru! Chcesz {qty}, masz {MaxAvailable}.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CartItems.Add(new DocumentItem
            {
                ProductId = CurrentProduct.Id,
                Quantity = qty,
                Product = CurrentProduct
            });

            if (CurrentDocType != "PZ") MaxAvailable -= qty;

            CurrentQuantity = "1";
            CurrentProduct = null;
        }

        [RelayCommand]
        private async Task AddNewContractorQuickly()
        {
            var vm = new ContractorEditorViewModel();
            var win = new ContractorWindow();
            vm.CloseAction = () => win.Close();
            win.DataContext = vm;
            win.ShowDialog();

            // Po zamknięciu okna przeładuj listę, żeby nowy kontrahent się pojawił
            await LoadDictionariesAsync();
        }

        [RelayCommand]
        private async Task SaveDocumentAsync()
        {
            if (CartItems.Count == 0) return;

            // Walidacja wyboru magazynów
            if (CurrentDocType == "PZ" && TargetWarehouse == null) { MessageBox.Show("Wybierz magazyn docelowy!"); return; }
            if (CurrentDocType == "WZ" && SourceWarehouse == null) { MessageBox.Show("Wybierz magazyn źródłowy!"); return; }
            if (CurrentDocType == "MM")
            {
                if (SourceWarehouse == null || TargetWarehouse == null) { MessageBox.Show("Wybierz oba magazyny!"); return; }
                if (SourceWarehouse.Id == TargetWarehouse.Id) { MessageBox.Show("Magazyn źródłowy i docelowy muszą być różne!"); return; }
            }

            try
            {
                using (var context = new UniaDbContext())
                {
                    var newDoc = new Document
                    {
                        Number = DocumentNumber,
                        DocType = CurrentDocType,
                        DocDate = DateTime.Now,
                        // Przypisujemy odpowiednie ID w zależności od tego, co jest wybrane
                        SourceWarehouseId = (CurrentDocType == "WZ" || CurrentDocType == "MM") ? SourceWarehouse.Id : null,
                        TargetWarehouseId = (CurrentDocType == "PZ" || CurrentDocType == "MM") ? TargetWarehouse.Id : null
                    };

                    context.Documents.Add(newDoc);
                    await context.SaveChangesAsync();

                    foreach (var item in CartItems)
                    {
                        context.DocumentItems.Add(new DocumentItem
                        {
                            DocumentId = newDoc.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });
                    }
                    await context.SaveChangesAsync();
                }

                MessageBox.Show($"Zapisano dokument {CurrentDocType}!");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                // ZMIANA: Wyciągamy "InnerException", czyli prawdziwy błąd MySQL
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Szczegóły błędu:\n{realError}", "Błąd Bazy Danych", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Tę metodę wywołujemy w SetMode
        private void FilterContractors()
        {
            FilteredContractors.Clear();

            if (CurrentDocType == "PZ")
            {
                // Pokaż tylko dostawców
                foreach (var c in _allContractors.Where(x => x.IsSupplier)) FilteredContractors.Add(c);
                ShowContractor = Visibility.Visible;
            }
            else if (CurrentDocType == "WZ")
            {
                // Pokaż tylko odbiorców
                foreach (var c in _allContractors.Where(x => x.IsRecipient)) FilteredContractors.Add(c);
                ShowContractor = Visibility.Visible;
            }
            else // MM
            {
                ShowContractor = Visibility.Collapsed; // Przy MM nie ma kontrahenta
            }
        }

        
        // Lista wszystkich z bazy
        private List<Contractor> _allContractors = new();

        // Lista widoczna w ComboBox (przefiltrowana)
        [ObservableProperty] private ObservableCollection<Contractor> _filteredContractors;

        // Wybrany kontrahent
        [ObservableProperty] private Contractor _selectedContractor;

        // Widoczność sekcji kontrahenta (dla MM ukrywamy)
        [ObservableProperty] private Visibility _showContractor = Visibility.Visible;
    }
}