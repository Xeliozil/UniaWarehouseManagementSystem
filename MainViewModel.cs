using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Product> _productsList;

        [ObservableProperty]
        private string _statusMessage = "Gotowy";

        public MainViewModel()
        {
            ProductsList = new ObservableCollection<Product>();

            // TEST: Dodajemy sztuczny produkt "na sztywno", bez bazy danych
            ProductsList.Add(new Product { Id = 999, Code = "TEST", Name = "To jest test widoku", Unit = "szt", MinStock = 0 });

            LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            StatusMessage = "£adowanie danych...";
            try
            {
                using (var context = new UniaDbContext())
                {
                    var list = await context.Products.ToListAsync();

                    ProductsList.Clear();
                    foreach (var item in list)
                    {
                        ProductsList.Add(item);
                    }
                }
                StatusMessage = $"Za³adowano {ProductsList.Count} produktów.";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"B³¹d po³¹czenia z baz¹:\n{ex.Message}", "B³¹d", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "B³¹d po³¹czenia";
            }
        }
    }
}