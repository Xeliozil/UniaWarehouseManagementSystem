using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class WarehouseEditorViewModel : ObservableObject
    {
        // Jeśli Id > 0, to znaczy, że edytujemy
        private int _warehouseId = 0;

        [ObservableProperty] private string _name;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _windowTitle = "Nowy Magazyn";

        public Action CloseAction { get; set; }

        // Konstruktor domyślny (dla nowego magazynu)
        public WarehouseEditorViewModel()
        {
        }

        // Konstruktor dla edycji (przyjmuje istniejący obiekt)
        public WarehouseEditorViewModel(Warehouse warehouse)
        {
            _warehouseId = warehouse.Id;
            Name = warehouse.Name;
            Description = warehouse.Description;
            WindowTitle = "Edycja Magazynu";
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Nazwa magazynu jest wymagana!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new UniaDbContext())
                {
                    if (_warehouseId == 0)
                    {
                        // --- DODAWANIE ---
                        var newWarehouse = new Warehouse
                        {
                            Name = Name,
                            Description = Description
                        };
                        context.Warehouses.Add(newWarehouse);
                    }
                    else
                    {
                        // --- EDYCJA ---
                        var existing = await context.Warehouses.FindAsync(_warehouseId);
                        if (existing != null)
                        {
                            existing.Name = Name;
                            existing.Description = Description;
                            // Entity Framework automatycznie wykryje zmiany
                        }
                    }

                    await context.SaveChangesAsync();
                }

                MessageBox.Show("Zapisano pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas zapisu:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}