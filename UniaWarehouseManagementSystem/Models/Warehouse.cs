using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniaWarehouseManagementSystem.Models
{
    [Table("Warehouses")]
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}