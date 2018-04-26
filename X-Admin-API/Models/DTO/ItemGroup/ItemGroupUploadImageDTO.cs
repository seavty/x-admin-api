using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupUploadImageDTO
    {

        [Required]
        public int id { get; set; }
        [Required]
        public List<string> base64s { get; set; }
    }
}