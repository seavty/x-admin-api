using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface IGetList<ListingDTO> where ListingDTO : class
    {
        Task<ListingDTO> GetList(int currentPage);
    }

    
}
