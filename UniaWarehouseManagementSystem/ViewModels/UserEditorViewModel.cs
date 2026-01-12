using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;
using UniaWarehouseManagementSystem.Services;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class UserEditorViewModel : ObservableObject
    {
        [ObservableProperty] private string _username;
        [ObservableProperty] private string _password; // Hasło jawne (do wpisania)
        [ObservableProperty] private string _selectedRole = "User";

        // Lista dostępnych ról
        public ObservableCollection<string> Roles { get; } = new() { "User", "Admin" };

        public Action CloseAction { get; set; }

        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Login i hasło są wymagane!");
                return;
            }

            try
            {
                using (var context = new UniaDbContext())
                {
                    // Sprawdzamy czy taki login już istnieje
                    if (context.Users.Any(u => u.Username == Username))
                    {
                        MessageBox.Show("Taki użytkownik już istnieje!");
                        return;
                    }

                    var newUser = new User
                    {
                        Username = Username,
                        Role = SelectedRole,
                        // TU SZYFRUJEMY HASŁO PRZED ZAPISEM:
                        PasswordHash = AuthService.HashPassword(Password)
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();
                }

                MessageBox.Show($"Dodano użytkownika {Username}!");
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu: " + ex.Message);
            }
        }
    }
}