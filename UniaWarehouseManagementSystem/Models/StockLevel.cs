using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniaWarehouseManagementSystem.Models
{
    [Table("StockLevels")]
    public class StockLevel
    {
        [Key]
        public int Id { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public Warehouse? Warehouse { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public decimal Quantity { get; set; }
    }
}