using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.SlideShow;

namespace X_Admin_API.Repository.Repo
{
    public class SlideShowRepository 
    {
        private THEntities db = null;

        public SlideShowRepository()
        {
            db = new THEntities();
        }

        //TODO : need to handle update event; delete / add new images

        //-> Create
        public async Task<SlideShowListDTO> Create(SlideShowNewDTO newDTO)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<sm_doc> documents = await Helper.Helper.SaveUploadImage(db,
                                                                                Helper.Helper.slideShowDocumentName,
                                                                                Helper.Helper.document_SlideShowTableID,
                                                                                Helper.Helper.slideShow_RecordID,
                                                                                newDTO.images);// tmp not useful , just reserve data for using in the furture
                    transaction.Commit();
                    return await SelectByID(Helper.Helper.slideShow_RecordID);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        //-> SelectByID
        public async Task<SlideShowListDTO> SelectByID(int id)
        {
            var documents = await (from d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == id)
                            select d).ToListAsync();

            if (documents.Count == 0)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");

            var slideShowList = new SlideShowListDTO();
            slideShowList.documents = Helper.Helper.GetDocuments(documents);
            return slideShowList;
        }
    }
}