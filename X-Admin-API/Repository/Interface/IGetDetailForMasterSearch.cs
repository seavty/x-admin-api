using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface IGetDetailForMasterSearch<ListingDTO> where ListingDTO : class
    {
        Task<ListingDTO> GetDetailForMasterSearch(int masterID, int currentPage, string search);
    }
}
