using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Customer
{
    public abstract class CustomerBase
    {
        [JsonProperty(Order = 1)]
        public int? id { get; set; }


        //Order = 2 for "code" -> check customer view dto class
 
        [JsonProperty(Order = 3)]
        [Required]
        [MaxLength(100)]
        public string name { get; set; }

        [JsonProperty(Order = 4)]
        [Required]
        [MaxLength(100)]
        public string phone { get; set; }

        [JsonProperty(Order = 5)]
        public string address { get; set; }
    }
}