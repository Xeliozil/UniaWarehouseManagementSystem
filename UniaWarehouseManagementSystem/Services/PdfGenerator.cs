using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.Services
{
    public class PdfGenerator
    {
        // 1. RAPORT ZBIORCZY (INWENTARYZACJA)
        public void GenerateStockReport(List<Product> products)
        {
            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Stan_Magazynowy_{DateTime.Now:yyyyMMdd_HHmm}.pdf");

            // Tutaj używamy QuestPDF.Fluent.Document
            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    var info = GetCompanyInfo();

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            // Używamy zmiennej 'info'
                            col.Item().Text(info.CompanyName).FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"NIP: {info.NIP}").FontSize(10);
                            col.Item().Text($"{info.Address}, {info.City}").FontSize(10);

                            col.Item().PaddingTop(5).Text("Raport Stanów Magazynowych").FontSize(14).Bold();
                            col.Item().Text($"Data: {DateTime.Now:g}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(40);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Lp.");
                            header.Cell().Element(CellStyle).Text("Kod");
                            header.Cell().Element(CellStyle).Text("Nazwa");
                            header.Cell().Element(CellStyle).Text("Ilość").AlignRight();
                            header.Cell().Element(CellStyle).Text("J.m.");
                            static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                        });

                        int index = 1;
                        foreach (var p in products)
                        {
                            table.Cell().Element(CellStyle).Text($"{index}");
                            table.Cell().Element(CellStyle).Text(p.Code);
                            table.Cell().Element(CellStyle).Text(p.Name);

                            // Logika koloru
                            var color = p.TotalQuantity < p.MinStock ? Colors.Red.Medium : Colors.Black;

                            table.Cell().Element(CellStyle).Text(p.TotalQuantity.ToString("0.##")).FontColor(color).AlignRight();
                            table.Cell().Element(CellStyle).Text(p.Unit);
                            static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            index++;
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            }).GeneratePdf(filename);

            OpenWithDefaultProgram(filename);
        }

        // 2. WYDRUK POJEDYNCZEGO DOKUMENTU
        // POPRAWKA TUTAJ: Pełna ścieżka do klasy Document (UniaWarehouseManagementSystem.Models.Document)
        public void GenerateDocumentPrintout(UniaWarehouseManagementSystem.Models.Document doc)
        {
            var safeNumber = doc.Number.Replace("/", "_");
            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Dokument_{safeNumber}.pdf");
            var info = GetCompanyInfo();

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    // NAGŁÓWEK DOKUMENTU
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            // Lewa: Dane Firmy
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(info.CompanyName).FontSize(18).Bold();
                                c.Item().Text(info.Address);
                                c.Item().Text(info.City);
                                c.Item().Text($"NIP: {info.NIP}");
                            });

                            // Prawa: Dane Dokumentu
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text(doc.DocType).FontSize(24).Bold().FontColor(Colors.Red.Medium);
                                c.Item().Text($"Nr: {doc.Number}").FontSize(14);
                                c.Item().Text($"Data: {doc.DocDate:yyyy-MM-dd}");
                            });
                        });

                        col.Item().PaddingVertical(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        // KONTRAHENT (Jeśli jest)
                        if (doc.Contractor != null)
                        {
                            col.Item().Text("Kontrahent:").Bold();
                            col.Item().Text(doc.Contractor.Name);
                            if (!string.IsNullOrEmpty(doc.Contractor.NIP)) col.Item().Text($"NIP: {doc.Contractor.NIP}");
                            col.Item().Text(doc.Contractor.Address ?? "");
                        }
                    });

                    // TABELA POZYCJI
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // Lp
                            columns.RelativeColumn();   // Nazwa
                            columns.ConstantColumn(80); // Ilość
                            columns.ConstantColumn(50); // Jm
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Lp.");
                            header.Cell().Element(HeaderStyle).Text("Nazwa Towaru");
                            header.Cell().Element(HeaderStyle).Text("Ilość").AlignRight();
                            header.Cell().Element(HeaderStyle).Text("J.m.");
                            static IContainer HeaderStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).Padding(5);
                        });

                        int i = 1;
                        foreach (var item in doc.Items)
                        {
                            table.Cell().Element(CellStyle).Text($"{i}");
                            table.Cell().Element(CellStyle).Text(item.Product?.Name ?? "Produkt usunięty");
                            table.Cell().Element(CellStyle).Text(item.Quantity.ToString("0.##")).AlignRight();
                            table.Cell().Element(CellStyle).Text(item.Product?.Unit ?? "");
                            static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                            i++;
                        }
                    });

                    // STOPKA Z PODPISAMI
                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingTop(50).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Black);
                                c.Item().AlignCenter().Text("Podpis Wystawiającego").FontSize(10);
                            });

                            row.ConstantItem(50); // Odstęp

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Black);
                                c.Item().AlignCenter().Text("Podpis Odbierającego").FontSize(10);
                            });
                        });

                        col.Item().PaddingTop(20).AlignCenter().Text(x =>
                        {
                            x.Span("Wygenerowano w systemie Unia WMS");
                        });
                    });
                });
            }).GeneratePdf(filename);

            OpenWithDefaultProgram(filename);
        }

        // 3. NOWOŚĆ: RAPORT RUCHÓW Z OKRESU
        public void GenerateMovementHistoryPdf(List<UniaWarehouseManagementSystem.Models.DocumentItem> items, DateTime from, DateTime to)
        {
            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Raport_Ruchow_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
            var info = GetCompanyInfo();
            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    // NAGŁÓWEK
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(info.CompanyName).FontSize(18).Bold();
                            c.Item().Text(info.Address);
                            c.Item().Text(info.City);
                            c.Item().Text($"NIP: {info.NIP}");
                        });
                    });

                    // TABELA
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80); // Data
                            columns.ConstantColumn(100); // Nr Dok
                            columns.ConstantColumn(40); // Typ
                            columns.RelativeColumn();    // Produkt
                            columns.ConstantColumn(60); // Ilość
                            columns.ConstantColumn(40); // Jm
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Data");
                            header.Cell().Element(HeaderStyle).Text("Nr Dok.");
                            header.Cell().Element(HeaderStyle).Text("Typ");
                            header.Cell().Element(HeaderStyle).Text("Produkt");
                            header.Cell().Element(HeaderStyle).Text("Ilość").AlignRight();
                            header.Cell().Element(HeaderStyle).Text("J.m.");

                            static IContainer HeaderStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).Padding(5);
                        });

                        foreach (var item in items)
                        {
                            var doc = item.Document; // Skrót
                            var date = doc.DocDate.ToString("yyyy-MM-dd");

                            // Kolorowanie: PZ na zielono, WZ na czerwono
                            var typeColor = doc.DocType == "PZ" ? Colors.Green.Medium : (doc.DocType == "WZ" ? Colors.Red.Medium : Colors.Black);

                            table.Cell().Element(CellStyle).Text(date);
                            table.Cell().Element(CellStyle).Text(doc.Number).FontSize(9);
                            table.Cell().Element(CellStyle).Text(doc.DocType).FontColor(typeColor).Bold();
                            table.Cell().Element(CellStyle).Text(item.Product?.Name ?? "---");
                            table.Cell().Element(CellStyle).Text(item.Quantity.ToString("0.##")).AlignRight();
                            table.Cell().Element(CellStyle).Text(item.Product?.Unit ?? "");

                            static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            }).GeneratePdf(filename);

            OpenWithDefaultProgram(filename);
        }

        private void OpenWithDefaultProgram(string filepath)
        {
            try { Process.Start(new ProcessStartInfo(filepath) { UseShellExecute = true }); } catch { }
        }
        private CompanyInfo GetCompanyInfo()
        {
            using (var context = new UniaDbContext())
            {
                // Pobierz pierwszy rekord lub domyślny, jeśli brak
                return context.CompanyInfos.FirstOrDefault() ?? new CompanyInfo
                {
                    CompanyName = "Brak Danych",
                    NIP = "",
                    Address = "",
                    City = ""
                };
            }
        }
    }
}