using Newtonsoft.Json;
using System.Collections.Generic;
using X_Admin_API.Models.DTO.Document;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupViewDTO : ItemGroupBase
    {
        
        [JsonProperty(Order = 3)]
        public List<DocumentViewDTO> documents { get; set; }
        
    }
}