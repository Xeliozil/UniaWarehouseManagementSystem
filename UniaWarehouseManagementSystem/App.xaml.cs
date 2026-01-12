using Microsoft.EntityFrameworkCore;
using System.Windows;
using UniaWarehouseManagementSystem.Models;
using UniaWarehouseManagementSystem.Services;
using UniaWarehouseManagementSystem.ViewModels;

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
            // To jest nasza pułapka na błędy krytyczne przy starcie
            try
            {
                base.OnStartup(e);

                // Tworzenie bazy
                using (var context = new Data.UniaDbContext())
                {
                    context.Database.EnsureCreated();

                    if (!System.Linq.Enumerable.Any(context.Users))
                    {
                        context.Users.Add(new Models.User
                        {
                            Username = "admin",
                            PasswordHash = Services.AuthService.HashPassword("admin"),
                            Role = "Admin"
                        });

                        if (!System.Linq.Enumerable.Any(context.CompanyInfos))
                        {
                            context.CompanyInfos.Add(new Models.CompanyInfo
                            {
                                CompanyName = "Moja Firma",
                                NIP = "-",
                                Address = "-",
                                City = "-"
                            });
                        }
                        context.SaveChanges();
                    }
                    // --- DODAJ TO: Ręczne tworzenie triggera dla SQLite ---
                    // SQLite wymaga nieco innej składni niż MySQL (brak IF/ELSEIF wewnątrz triggera w prosty sposób,
                    // często robi się oddzielne triggery lub używa składni CASE/WHEN w UPDATE).
                    // Poniżej uproszczona wersja, która powinna zadziałać dla SQLite:

                    try
                    {
                        // Trigger dla PZ (Przyjęcie Zewnętrzne) - zwiększa stan
                        context.Database.ExecuteSqlRaw(@"
                            CREATE TRIGGER IF NOT EXISTS After_DocumentItem_Insert_PZ
                            AFTER INSERT ON DocumentItems
                            WHEN (SELECT DocType FROM Documents WHERE Id = NEW.DocumentId) = 'PZ'
                            BEGIN
                                INSERT INTO StockLevels (WarehouseId, ProductId, Quantity)
                                SELECT TargetWarehouseId, NEW.ProductId, NEW.Quantity
                                FROM Documents WHERE Id = NEW.DocumentId
                                ON CONFLICT(WarehouseId, ProductId) DO UPDATE SET Quantity = Quantity + NEW.Quantity;
                            END;
                        ");

                        // Trigger dla WZ (Wydanie Zewnętrzne) - zmniejsza stan
                        context.Database.ExecuteSqlRaw(@"
                            CREATE TRIGGER IF NOT EXISTS After_DocumentItem_Insert_WZ
                            AFTER INSERT ON DocumentItems
                            WHEN (SELECT DocType FROM Documents WHERE Id = NEW.DocumentId) = 'WZ'
                            BEGIN
                                UPDATE StockLevels 
                                SET Quantity = Quantity - NEW.Quantity
                                WHERE ProductId = NEW.ProductId 
                                AND WarehouseId = (SELECT SourceWarehouseId FROM Documents WHERE Id = NEW.DocumentId);
                            END;
                        ");

                        // Trigger dla MM (Przesunięcie) - wymagałby bardziej złożonej logiki, 
                        // w SQLite lepiej obsłużyć MM w kodzie C# lub rozbić na dwa operacje (zdejmij/dodaj).
                    }
                    catch (System.Exception ex)
                    {
                        // Ignorujemy błędy jeśli triggery już istnieją lub coś poszło nie tak
                        System.Diagnostics.Debug.WriteLine("Błąd tworzenia triggerów: " + ex.Message);
                    }
                }

                ShowLoginWindow();
            }
            catch (System.Exception ex)
            {
                // JEŚLI COŚ PÓJDZIE NIE TAK, ZOBACZYSZ TEN KOMUNIKAT:
                MessageBox.Show($"Błąd krytyczny przy starcie:\n\n{ex.Message}\n\n{ex.StackTrace}",
                                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Zamykamy, bo i tak nie zadziała
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
            // (Musimy to zrobić ręcznie, bo zmieniliśmy ShutdownMode na Explicit)
            loginWindow.Closed += (s, e) =>
            {
                // Sprawdzamy czy okno zamknęło się po sukcesie (wtedy nic nie rób), 
                // czy po prostu ktoś kliknął X (wtedy zamknij apkę)
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
                // Fallback (gdyby VM był tworzony ręcznie)
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
                // Jeśli okno się zamknęło, a NIE kliknięto "Wyloguj", to znaczy, że użytkownik chce wyjść.
                if (!isLoggingOut)
                {
                    Shutdown(); // Zabij proces
                }
            };

            mainWindow.Show();
        }
    }
}