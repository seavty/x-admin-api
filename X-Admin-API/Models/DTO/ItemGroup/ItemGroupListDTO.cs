using System.Collections.Generic;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupListDTO
    {
        public MetaData metaData { get; set; }
        public List<ItemGroupViewDTO> results { get; set; }
    }
}