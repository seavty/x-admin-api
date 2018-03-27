using System.Collections.Generic;

namespace X_Admin_API.Models.DTO.Item
{
    public class ItemListDTO
    {
        public MetaData metaData { get; set; }
        public List<ItemViewDTO> results { get; set; }
    }
}