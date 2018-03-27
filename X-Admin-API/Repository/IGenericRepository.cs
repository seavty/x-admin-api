using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_Admin_API.Repository
{
    public interface IOperation<T> where T : class
    {
        void Insert(T obj);
        void Update(T obj);
        void Delete(int id);
        void Save();

    }
}
