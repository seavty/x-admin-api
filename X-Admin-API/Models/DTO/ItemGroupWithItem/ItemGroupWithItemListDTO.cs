using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.ItemGroupWithItem
{
    public class ItemGroupWithItemListDTO
    {
        public MetaData metaData { get; set; }
        public List<ItemGroupWithItemViewDTO> results { get; set; }
    }
}