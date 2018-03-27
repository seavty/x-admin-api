using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace X_Admin_API.Models.DTO.IssueItem
{
    public class IsseItemNewDTO
    {
        [Required]
        public int warehouseID { get; set; }

        [MaxLength(1000)]
        public string remarks { get; set; }

        [Required]
        public List<IssueItemBase> items { get; set; }
    }
}