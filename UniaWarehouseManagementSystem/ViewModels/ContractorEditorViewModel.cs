using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class ContractorEditorViewModel : ObservableObject
    {
        [ObservableProperty] private string _name;

        // POPRAWKA: Używamy _NIP (dużymi), żeby wygenerowało public string NIP
        [ObservableProperty] private string _NIP;

        [ObservableProperty] private string _address;
        [ObservableProperty] private bool _isSupplier = true;
        [ObservableProperty] private bool _isRecipient = true;
        [ObservableProperty] private string _windowTitle = "Nowy Kontrahent";

        public Action CloseAction { get; set; }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Nazwa jest wymagana!"); return;
            }

            try
            {
                using (var context = new UniaDbContext())
                {
                    var newContractor = new Contractor
                    {
                        Name = Name,
                        // Teraz to zadziała, bo właściwość nazywa się NIP
                        NIP = NIP,
                        Address = Address,
                        IsSupplier = IsSupplier,
                        IsRecipient = IsRecipient
                    };
                    context.Contractors.Add(newContractor);
                    await context.SaveChangesAsync();
                }

                MessageBox.Show("Dodano kontrahenta!");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: " + ex.Message);
            }
        }
    }
}