using System.ComponentModel.DataAnnotations;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupDTO
    {
        public int? id { get; set; }
       
        [Required]
        [MaxLength(100)]
        public string name { get; set; }

    }
}