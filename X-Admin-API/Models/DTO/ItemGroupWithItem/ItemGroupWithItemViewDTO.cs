using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO.Item;

namespace X_Admin_API.Models.DTO.ItemGroupWithItem
{
    public class ItemGroupWithItemViewDTO : ItemGroupWithItemBase
    {
        [JsonProperty(Order = 4)]
        public List<ItemViewForMasterDTO> items { get; set; }
    }
}