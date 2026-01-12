using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class WarehouseListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Warehouse> _warehouses = new ObservableCollection<Warehouse>();

        public WarehouseListViewModel()
        {
            _ = LoadWarehouses();
        }

        [RelayCommand]
        public async Task LoadWarehouses()
        {
            using (var context = new UniaDbContext())
            {
                var list = await context.Warehouses.ToListAsync();
                Warehouses = new ObservableCollection<Warehouse>(list);
            }
        }

        [RelayCommand]
        private void AddWarehouse()
        {
            // Otwieramy okno edytora (to z poprzedniego kroku)
            var vm = new WarehouseEditorViewModel();
            var window = new WarehouseWindow(vm);

            // Gdy zamkniemy edytor, odświeżamy listę
            vm.CloseAction = () =>
            {
                window.Close();
                _ = LoadWarehouses();
            };

            window.ShowDialog();
        }

        [RelayCommand]
        private void EditWarehouse(Warehouse warehouse)
        {
            if (warehouse == null) return;

            var vm = new WarehouseEditorViewModel(warehouse);
            var window = new WarehouseWindow(vm);

            vm.CloseAction = () =>
            {
                window.Close();
                _ = LoadWarehouses();
            };

            window.ShowDialog();
        }
    }
}