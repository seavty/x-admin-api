using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface IGetDetailForMaster<ListingDTO> where ListingDTO : class
    {
        Task<ListingDTO> GetDetailForMaster(int masterID,int currentPage);
    }
}
