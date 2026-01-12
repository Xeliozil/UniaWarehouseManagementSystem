using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniaWarehouseManagementSystem.Models
{
    [Table("CompanyInfo")]
    public class CompanyInfo
    {
        [Key]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string NIP { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}