using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniaWarehouseManagementSystem.Models
{
    [Table("Documents")]
    public class Document
    {
        [Key]
        public int Id { get; set; }

        public string Number { get; set; } = string.Empty;
        public string DocType { get; set; } = "PZ";
        public DateTime DocDate { get; set; } = DateTime.Now;

        public int? TargetWarehouseId { get; set; }
        public int? SourceWarehouseId { get; set; }

        // --- NOWE POLA ---
        public int? ContractorId { get; set; }

        [ForeignKey("ContractorId")]
        public virtual Contractor? Contractor { get; set; }
        // -----------------

        public List<DocumentItem> Items { get; set; } = new();
    }
}