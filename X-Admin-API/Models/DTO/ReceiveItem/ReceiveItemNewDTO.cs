using X_Admin_API.Models.DTO.Item;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace X_Admin_API.Models.DTO.ReceiveItem
{
    public class ReceiveItemNewDTO
    {
        [Required]
        public int warehouseID { get; set; }

        [MaxLength(1000)]
        public string remarks { get; set; }

        [Required]
        public List<ReceiveItemBase> items { get; set; }
    }
}