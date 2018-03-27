using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace X_Admin_API.Models.DTO.ReceiveItem
{
    public class ReceiveItemBase
    {
        [Required]
        public int id { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public double cost { get; set; }

    }
}