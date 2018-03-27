using System.ComponentModel.DataAnnotations;

namespace X_Admin_API.Models.DTO.IssueItem
{
    public class IssueItemBase
    {
        [Required]
        public int id { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public double price { get; set; }
    }
}