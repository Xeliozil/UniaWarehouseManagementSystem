using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;
using UniaWarehouseManagementSystem.Services;

namespace UniaWarehouseManagementSystem.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Document> _documents;
        [ObservableProperty] private Document _selectedDocument;
        [ObservableProperty] private string _status;

        // --- FILTRY ---

        // Daty (Zmiana daty automatycznie uruchamia LoadDocumentsAsync)
        [ObservableProperty] private DateTime _dateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        [ObservableProperty] private DateTime _dateTo = DateTime.Now;

        // Typ Dokumentu
        [ObservableProperty] private string _selectedDocType = "Wszystkie";
        public ObservableCollection<string> DocTypes { get; } = new() { "Wszystkie", "PZ", "WZ", "MM" };

        // Kontrahent
        [ObservableProperty] private Contractor? _selectedContractor; // Null oznacza "Wszyscy"
        [ObservableProperty] private ObservableCollection<Contractor> _contractors; // Lista do wyboru

        public HistoryViewModel()
        {
            Documents = new ObservableCollection<Document>();
            Contractors = new ObservableCollection<Contractor>();

            // Ładujemy kontrahentów do filtra
            LoadContractors();

            // Ładujemy dokumenty na start
            LoadDocumentsAsync();
        }

        private void LoadContractors()
        {
            using (var ctx = new UniaDbContext())
            {
                var list = ctx.Contractors.OrderBy(c => c.Name).ToList();
                Contractors.Clear();
                // Opcjonalnie: Można dodać sztucznego kontrahenta "Wszyscy", ale prościej obsłużyć null
                foreach (var c in list) Contractors.Add(c);
            }
        }

        // --- AUTOMATYCZNE ODŚWIEŻANIE ---
        // Te metody (partial) są generowane przez CommunityToolkit, gdy zmieniasz właściwość z [ObservableProperty]
        partial void OnDateFromChanged(DateTime value) => LoadDocumentsAsync();
        partial void OnDateToChanged(DateTime value) => LoadDocumentsAsync();
        partial void OnSelectedDocTypeChanged(string value) => LoadDocumentsAsync();
        partial void OnSelectedContractorChanged(Contractor? value) => LoadDocumentsAsync();

        [RelayCommand]
        private async Task LoadDocumentsAsync()
        {
            Status = "Filtrowanie...";
            var realTo = DateTo.Date.AddDays(1).AddSeconds(-1); // Koniec dnia

            using (var context = new UniaDbContext())
            {
                // Budujemy zapytanie
                var query = context.Documents
                    .Include(d => d.Contractor)
                    .Include(d => d.Items).ThenInclude(i => i.Product)
                    .Where(d => d.DocDate >= DateFrom && d.DocDate <= realTo);

                // Filtr Typu
                if (SelectedDocType != "Wszystkie")
                {
                    query = query.Where(d => d.DocType == SelectedDocType);
                }

                // Filtr Kontrahenta
                if (SelectedContractor != null)
                {
                    query = query.Where(d => d.ContractorId == SelectedContractor.Id);
                }

                var result = await query.OrderByDescending(d => d.DocDate).ToListAsync();

                Documents.Clear();
                foreach (var d in result) Documents.Add(d);
            }
            Status = $"Znaleziono {Documents.Count} dokumentów.";
        }

        [RelayCommand]
        private void PrintDocument()
        {
            if (SelectedDocument == null) { MessageBox.Show("Wybierz dokument!"); return; }
            new PdfGenerator().GenerateDocumentPrintout(SelectedDocument);
        }

        // --- RAPORT ZBIORCZY (Z FILTRAMI) ---
        [RelayCommand]
        private async Task GeneratePeriodReport()
        {
            Status = "Generowanie PDF...";
            var realTo = DateTo.Date.AddDays(1).AddSeconds(-1);

            try
            {
                using (var context = new UniaDbContext())
                {
                    // Musimy pobrać pozycje (Items) pasujące do filtrów dokumentów
                    var query = context.DocumentItems
                        .Include(i => i.Document)
                        .Include(i => i.Product)
                        .Where(i => i.Document.DocDate >= DateFrom && i.Document.DocDate <= realTo);

                    // Te same filtry co dla listy
                    if (SelectedDocType != "Wszystkie")
                        query = query.Where(i => i.Document.DocType == SelectedDocType);

                    if (SelectedContractor != null)
                        query = query.Where(i => i.Document.ContractorId == SelectedContractor.Id);

                    var items = await query.OrderBy(i => i.Document.DocDate).ToListAsync();

                    if (items.Count == 0)
                    {
                        MessageBox.Show("Brak danych spełniających kryteria!");
                        return;
                    }

                    var generator = new PdfGenerator();
                    generator.GenerateMovementHistoryPdf(items, DateFrom, realTo);
                }
                Status = "PDF gotowy.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd: {ex.Message}");
            }
        }

        // Komenda do czyszczenia filtra kontrahenta (przycisk "X")
        [RelayCommand]
        private void ClearContractor()
        {
            SelectedContractor = null;
        }
    }
}