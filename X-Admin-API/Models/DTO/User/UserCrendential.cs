using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.User
{
    public class UserCrendential
    {
        [Required]
        [MaxLength(50)]
        public string userName { get; set; }

        //TODO: enable it when in deploy
        //[Required] // for testing no need to use password
        [MaxLength(50)]
        public string password { get; set; }
    }
}