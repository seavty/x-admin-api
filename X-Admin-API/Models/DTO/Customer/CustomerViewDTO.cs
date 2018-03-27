using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Customer
{
    public class CustomerViewDTO : CustomerBase
    {
        [JsonProperty(Order = 2)]
        public string code { get; set; }
    }
}