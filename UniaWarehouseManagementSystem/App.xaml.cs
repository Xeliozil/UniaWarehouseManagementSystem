using Microsoft.EntityFrameworkCore;
using System.Windows;
using UniaWarehouseManagementSystem.Models;
using UniaWarehouseManagementSystem.Services;
using UniaWarehouseManagementSystem.ViewModels;
using System.Linq; // Dodane dla czytelniejszego .Any()

namespace UniaWarehouseManagementSystem
{
    public partial class App : Application
    {
        public App()
        {
            // Licencja darmowa dla biblioteki PDF
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // To jest nasza pułapka na błędy krytyczne przy starcie
            try
            {
                base.OnStartup(e);

                // Tworzenie/Inicjalizacja bazy danych SQLite
                using (var context = new Data.UniaDbContext())
                {
                    // Ta linia stworzy plik UniaWarehouse.db, jeśli go nie ma
                    context.Database.EnsureCreated();

                    // --- SEEDOWANIE DANYCH (Domyślny Admin i Firma) ---

                    // 1. Domyślny użytkownik (admin/admin)
                    if (!context.Users.Any())
                    {
                        context.Users.Add(new Models.User
                        {
                            Username = "admin",
                            PasswordHash = Services.AuthService.HashPassword("admin"),
                            Role = "Admin"
                        });
                        // Zapisujemy od razu, żeby mieć pewność
                        context.SaveChanges();
                    }

                    // 2. Domyślne dane firmy (puste)
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

                    // UWAGA: USUNIĘTO TWORZENIE TRIGGERÓW
                    // Logika aktualizacji stanów (PZ/WZ/MM) znajduje się teraz w DocumentEditorViewModel.cs
                    // Dzięki temu unikamy podwójnego naliczania towarów (raz przez C#, raz przez SQL).
                }

                ShowLoginWindow();
            }
            catch (System.Exception ex)
            {
                // JEŚLI COŚ PÓJDZIE NIE TAK (np. brak uprawnień do zapisu pliku .db), ZOBACZYSZ TEN KOMUNIKAT:
                MessageBox.Show($"Błąd krytyczny przy starcie:\n\n{ex.Message}\n\n{ex.StackTrace}",
                                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Zamykamy, bo i tak nie zadziała bez bazy
                Shutdown();
            }
        }

        // Metoda 1: Pokaż Logowanie
        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var loginVm = new LoginViewModel();

            // Co zrobić, jak logowanie się uda? -> Otwórz Główne
            loginVm.OpenMainAction = () =>
            {
                loginWindow.Close();
                ShowMainWindow();
            };

            // Co zrobić, jak ktoś kliknie "Wyjście" w oknie logowania? -> Zabij aplikację
            loginWindow.Closed += (s, e) =>
            {
                // Sprawdzamy czy użytkownik jest zalogowany (czy okno zamknęło się po sukcesie)
                // Jeśli nie jest zalogowany i okno się zamknęło -> znaczy, że kliknął X
                if (AuthService.CurrentUser == null)
                {
                    Shutdown();
                }
            };

            loginWindow.DataContext = loginVm;
            loginWindow.Show();
        }

        // Metoda 2: Pokaż Główne Okno
        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow();

            // Flaga, która mówi nam, czy zamknięcie jest celowym wylogowaniem
            bool isLoggingOut = false;

            if (mainWindow.DataContext is MainViewModel mainVm)
            {
                // Logika wylogowania
                mainVm.RequestLogoutAction = () =>
                {
                    isLoggingOut = true; // Podnosimy flagę: "To jest wylogowanie, nie zamykaj procesu!"
                    mainWindow.Close();
                    ShowLoginWindow();
                };
            }
            else
            {
                // Fallback (gdyby VM był tworzony ręcznie w XAML, a nie wstrzykiwany, choć tu robimy to w kodzie)
                var vm = new MainViewModel();
                vm.RequestLogoutAction = () =>
                {
                    isLoggingOut = true;
                    mainWindow.Close();
                    ShowLoginWindow();
                };
                mainWindow.DataContext = vm;
            }

            // OBSŁUGA ZAMKNIĘCIA (X lub Alt+F4)
            mainWindow.Closed += (sender, args) =>
            {
                // Jeśli okno się zamknęło, a NIE kliknięto "Wyloguj", to znaczy, że użytkownik chce wyjść z programu.
                if (!isLoggingOut)
                {
                    Shutdown(); // Zabij proces
                }
            };

            mainWindow.Show();
        }
    }
}