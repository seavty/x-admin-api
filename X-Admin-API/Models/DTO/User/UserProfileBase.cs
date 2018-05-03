using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO.Setting;

namespace X_Admin_API.Models.DTO.User
{
    public abstract class UserProfileBase
    {
        public int? id { get; set; }

        [Required]
        [MaxLength(50)]
        public string userName { get; set; }

        [Required]
        [MaxLength(50)]
        public string firstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string lastName { get; set; }

        [Required]
        [MaxLength(10)]
        public string gender { get; set; }

        public SettingViewDTO setting { get; set; }

        public int user_SalesmanID { get; set; }
    }
}