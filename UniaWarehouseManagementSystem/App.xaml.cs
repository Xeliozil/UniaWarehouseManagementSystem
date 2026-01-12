using Microsoft.EntityFrameworkCore;
using System.Windows;
using UniaWarehouseManagementSystem.Models;
using UniaWarehouseManagementSystem.Services;
using UniaWarehouseManagementSystem.ViewModels;
using System.Linq;

namespace UniaWarehouseManagementSystem
{
    public partial class App : Application
    {
        public App()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                using (var context = new Data.UniaDbContext())
                {
                    context.Database.EnsureCreated();

                    // Seedowanie użytkownika admin
                    if (!context.Users.Any())
                    {
                        context.Users.Add(new Models.User
                        {
                            Username = "admin",
                            PasswordHash = Services.AuthService.HashPassword("admin"),
                            Role = "Admin"
                        });
                        context.SaveChanges();
                    }

                    // Seedowanie danych firmy
                    if (!context.CompanyInfos.Any())
                    {
                        context.CompanyInfos.Add(new Models.CompanyInfo
                        {
                            CompanyName = "Moja Firma",
                            NIP = "-",
                            Address = "-",
                            City = "-"
                        });
                        context.SaveChanges();
                    }

                    // LOGIKA TRIGGERÓW ZOSTAŁA USUNIĘTA.
                    // Wszystkie operacje na stanach magazynowych wykonuje kod C#
                    // w klasie DocumentEditorViewModel.cs.
                }

                ShowLoginWindow();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Błąd krytyczny przy starcie:\n\n{ex.Message}",
                                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var loginVm = new LoginViewModel();

            loginVm.OpenMainAction = () =>
            {
                loginWindow.Close();
                ShowMainWindow();
            };

            loginWindow.Closed += (s, e) =>
            {
                if (AuthService.CurrentUser == null) Shutdown();
            };

            loginWindow.DataContext = loginVm;
            loginWindow.Show();
        }

        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow();
            bool isLoggingOut = false;

            if (mainWindow.DataContext is MainViewModel mainVm)
            {
                mainVm.RequestLogoutAction = () =>
                {
                    isLoggingOut = true;
                    mainWindow.Close();
                    ShowLoginWindow();
                };
            }

            mainWindow.Closed += (sender, args) =>
            {
                if (!isLoggingOut) Shutdown();
            };

            mainWindow.Show();
        }
    }
}