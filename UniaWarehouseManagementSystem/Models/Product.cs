using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // Bardzo ważne - pozwala użyć .Sum()

namespace UniaWarehouseManagementSystem.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        // Używamy decimal, tak jak w Twoim kodzie
        public decimal MinStock { get; set; }

        // Przywrócone pole opisu
        public string? Description { get; set; }

        // --- KLUCZ DO POPRAWNEGO DZIAŁANIA ---
        // Dodajemy relację do stanów magazynowych.
        // Dzięki temu Entity Framework pobierze listę stanów dla tego produktu.
        public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();

        // --- AUTOMATYCZNE OBLICZANIE SUMY ---
        // [NotMapped] - nie tworzy kolumny w bazie.
        // Używamy strzałki '=>', aby wartość liczyła się na żywo z listy StockLevels.
        [NotMapped]
        public decimal TotalQuantity => StockLevels?.Sum(s => s.Quantity) ?? 0;

        // Właściwość pomocnicza dla koloru (czerwony gdy mało towaru)
        [NotMapped]
        public bool IsLowStock => TotalQuantity < MinStock;
    }
}