using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public decimal MinStock { get; set; }
        public string? Description { get; set; }
    }
}