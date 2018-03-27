using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository.Interface
{
    public interface IEdit<EditDTO, ViewDTO> where EditDTO : class
                                             where ViewDTO : class
    {
        Task<ViewDTO> Edit(EditDTO editDTO);
    }

}
