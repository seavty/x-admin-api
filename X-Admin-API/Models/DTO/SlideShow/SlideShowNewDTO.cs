using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.SlideShow
{
    public class SlideShowNewDTO : SlideShowBase
    {
        public List<string> images { get; set; }
    }
}