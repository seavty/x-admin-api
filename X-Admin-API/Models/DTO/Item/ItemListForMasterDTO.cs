using System.Collections.Generic;

namespace X_Admin_API.Models.DTO.Item
{
    public class ItemListForMasterDTO
    {
        public MetaData metaData { get; set; }
        public List<ItemViewForMasterDTO> results { get; set; }
    }
}