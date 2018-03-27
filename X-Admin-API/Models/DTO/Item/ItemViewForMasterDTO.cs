using Newtonsoft.Json;
using System.Collections.Generic;
using X_Admin_API.Models.DTO.Document;

namespace X_Admin_API.Models.DTO.Item
{
    public class ItemViewForMasterDTO: ItemBase
    {
        [JsonProperty(Order = 5)]
        public int quantity { get; set; }

        [JsonProperty(Order = 6)]
        public double price { get; set; }

        [JsonProperty(Order = 7)]
        public double cost { get; set; }

        [JsonProperty(Order = 8)]
        public double lastCost { get; set; }

        [JsonProperty(Order = 9)]
        public List<DocumentViewDTO> documents { get; set; }

    }
}