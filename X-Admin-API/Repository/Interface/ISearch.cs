using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface ISearch<Listing> where Listing : class
    {
        Task<Listing> Search(int currentPage, string search);
    }

}
