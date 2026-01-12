using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniaWarehouseManagementSystem.Models
{
    [Table("DocumentItems")]
    public class DocumentItem
    {
        [Key]
        public int Id { get; set; }

        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public decimal Quantity { get; set; }
    }
}