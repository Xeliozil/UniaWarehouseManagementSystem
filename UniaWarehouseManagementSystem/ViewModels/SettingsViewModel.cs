using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] private string _companyName;

        // POPRAWKA: _NIP (dużymi), żeby wygenerowało public string NIP
        [ObservableProperty] private string _NIP;

        [ObservableProperty] private string _address;
        [ObservableProperty] private string _city;

        public SettingsViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            using (var context = new UniaDbContext())
            {
                var info = await context.CompanyInfos.FirstOrDefaultAsync();
                if (info != null)
                {
                    CompanyName = info.CompanyName;
                    // Teraz zadziała, bo właściwość nazywa się NIP
                    NIP = info.NIP;
                    Address = info.Address;
                    City = info.City;
                }
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            using (var context = new UniaDbContext())
            {
                var info = await context.CompanyInfos.FirstOrDefaultAsync();
                if (info == null)
                {
                    info = new CompanyInfo();
                    context.CompanyInfos.Add(info);
                }

                info.CompanyName = CompanyName;
                info.NIP = NIP; // Teraz zadziała
                info.Address = Address;
                info.City = City;

                await context.SaveChangesAsync();
            }
            MessageBox.Show("Dane firmy zostały zaktualizowane!");
        }
    }
}