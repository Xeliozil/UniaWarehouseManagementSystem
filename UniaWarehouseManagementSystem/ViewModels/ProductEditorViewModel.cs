using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class ProductEditorViewModel : ObservableObject
    {
        // Pola formularza
        [ObservableProperty] private string _code;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _unit;
        [ObservableProperty] private decimal _minStock;
        [ObservableProperty] private string _description;

        // Lista jednostek do wyboru w ComboBox
        public ObservableCollection<string> AvailableUnits { get; } = new()
        {
            "szt", "kg", "m", "L", "kpl", "opak", "m2"
        };

        [ObservableProperty] private string _windowTitle = "Nowy Produkt";

        private int? _editingProductId = null; // Jeśli null = Tryb Dodawania, Jeśli liczba = Tryb Edycji
        public Action CloseAction { get; set; }

        // Konstruktor domyślny (dla Dodawania)
        public ProductEditorViewModel()
        {
            Unit = "szt"; // Domyślna jednostka
        }

        // Metoda do załadowania danych (dla Edycji)
        public void LoadProduct(Product product)
        {
            if (product == null) return;

            WindowTitle = "Edycja Produktu: " + product.Code;
            _editingProductId = product.Id;

            // Przepisujemy dane do pól formularza
            Code = product.Code;
            Name = product.Name;
            Unit = product.Unit;
            MinStock = product.MinStock;
            Description = product.Description;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Prosta walidacja
            if (string.IsNullOrWhiteSpace(Code) || string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Kod i Nazwa są wymagane!");
                return;
            }

            try
            {
                using (var context = new UniaDbContext())
                {
                    if (_editingProductId == null)
                    {
                        // TRYB: NOWY PRODUKT
                        var newProduct = new Product
                        {
                            Code = Code,
                            Name = Name,
                            Unit = Unit,
                            MinStock = MinStock,
                            Description = Description
                        };
                        context.Products.Add(newProduct);
                    }
                    else
                    {
                        // TRYB: EDYCJA
                        var productToUpdate = await context.Products.FindAsync(_editingProductId);
                        if (productToUpdate != null)
                        {
                            productToUpdate.Code = Code;
                            productToUpdate.Name = Name;
                            productToUpdate.Unit = Unit;
                            productToUpdate.MinStock = MinStock;
                            productToUpdate.Description = Description;
                        }
                    }

                    await context.SaveChangesAsync();
                }

                MessageBox.Show("Zapisano produkt!");
                CloseAction?.Invoke();
            }
            catch (DbUpdateException ex)
            {
                // Sprawdzamy czy to błąd duplikatu (np. ten sam Kod)
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    MessageBox.Show($"Produkt o kodzie '{Code}' już istnieje! Zmień kod.", "Duplikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Błąd bazy danych: " + ex.Message);
                }
            }
        }
    }
}