using System.Collections.Generic;

namespace X_Admin_API.Models.DTO.Customer
{
    public class CustomerListDTO
    {
        public MetaData metaData { get; set; }
        public List<CustomerViewDTO> results { get; set; }
    }
}