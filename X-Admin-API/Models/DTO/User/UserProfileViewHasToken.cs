using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace X_Admin_API.Models.DTO.User
{
    public class UserProfileViewHasToken
    {

        public UserProfileViewDTO userProfile { get; set; }

        [MaxLength(100)]
        public string token { get; set; }
    }
}