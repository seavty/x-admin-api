using X_Admin_API.Models.DTO.Customer;
using X_Admin_API.Models.DTO.Warehouse;

namespace X_Admin_API.Models.DTO.Setting
{
    public class SettingViewDTO
    {
        public int? id { get; set; }
        public CustomerViewDTO customer { get; set; }
        public WarehouseViewDTO warehouse { get; set; }
    }
}