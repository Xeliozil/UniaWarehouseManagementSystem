using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniaWarehouseManagementSystem.Models
{
    [Table("Contractors")]
    public class Contractor
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? NIP { get; set; }
        public string? Address { get; set; }

        // Flagi: Czy to dostawca, czy odbiorca (może być oboma naraz)
        public bool IsSupplier { get; set; } = true;
        public bool IsRecipient { get; set; } = true;

        public string? Description { get; set; }

        // Pomocnicze pole do wyświetlania w ComboBox (np. "Hurtownia (NIP: ...)")
        [NotMapped]
        public string DisplayName => string.IsNullOrEmpty(NIP) ? Name : $"{Name} (NIP: {NIP})";
    }
}