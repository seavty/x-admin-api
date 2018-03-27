using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface IMasterDetailList<MaserDetailList> where MaserDetailList : class
    {
        Task<MaserDetailList> GetMasterDetailList(int currentPage);
    }
}
