using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DB;

namespace X_Admin_API.Repository.Repo
{
    public class DocumentRepository
    {
        private THEntities db = null;

        public DocumentRepository()
        {
            db = new THEntities();
        }

        //-> Delete
        public async Task<Boolean> Delete(int id)
        {
            var record = await db.sm_doc.FirstOrDefaultAsync(x => x.docs_Deleted == null && x.id == id);
            if (record == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            record.docs_Deleted = "1";
            await db.SaveChangesAsync();
            return true;
        }
    }
}