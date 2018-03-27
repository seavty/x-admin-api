using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface ISelectByID <ViewDTO> where ViewDTO : class
    {
        Task<ViewDTO> SelectByID(int id);
    }
}
