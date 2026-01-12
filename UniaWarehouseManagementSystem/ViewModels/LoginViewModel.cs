using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Controls; // Potrzebne do PasswordBox
using UniaWarehouseManagementSystem.Services;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] private string _username;

        public Action CloseAction { get; set; } // Zamyka okno logowania
        public Action OpenMainAction { get; set; } // Otwiera główne okno

        // PasswordBox nie obsługuje bindowania (ze względów bezpieczeństwa),
        // więc przekazujemy go jako parametr do komendy.
        [RelayCommand]
        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password ?? "";

            if (AuthService.Login(Username, password))
            {
                // Sukces!
                OpenMainAction?.Invoke();
                CloseAction?.Invoke();
            }
            else
            {
                MessageBox.Show("Błędny login lub hasło!", "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        [RelayCommand]
        private void Exit()
        {
            // Wywołujemy akcję zamknięcia okna.
            // Ponieważ nie jesteśmy zalogowani, App.xaml.cs wykryje to i zamknie proces aplikacji.
            Application.Current.Shutdown();
        }
    }
}