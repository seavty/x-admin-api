using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.Document;
using System.Data.Entity;
using System.IO;
using System.Drawing;
using System.Web.Configuration;

namespace X_Admin_API.Helper
{
    public class DocumentHelper
    {
        //-> Get Document
        public static List<DocumentViewDTO> GetDocuments(THEntities db, int tableID, int value)
        {
            List<DocumentViewDTO> documentViews = new List<DocumentViewDTO>();
            IQueryable<sm_doc> documents = from d in db.sm_doc
                                           where d.docs_Deleted == null && d.tableID == tableID &&  d.value == value.ToString()
                                           orderby d.id ascending
                                           select d;
            foreach (var document in documents)
            {
                var mappingDTO = MappingHelper.MapDBClassToDTO<sm_doc, DocumentViewDTO>(document);
                //documentViews.Add(MappingHelper.MapDBClassToDTO<sm_doc, DocumentViewDTO>(document));
                mappingDTO.path = WebConfigurationManager.AppSettings["baseURL"].ToString() + mappingDTO.path; 
                documentViews.Add(mappingDTO);
            }

            return documentViews;
        }

        public static async Task<List<sm_doc>> SaveUploadImage(THEntities db, int tableID, int recordID, List<string> base64)
        {
            List<sm_doc> documents = new List<sm_doc>();
            if (base64 != null)
            {
                foreach (var image in base64)
                {
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(image)))
                    {
                        string pathForSavingToDB = "", imageNameForSavingToDB = "";
                        using (Bitmap bm = new Bitmap(ms))
                        {
                            //string path = "";
                            string year = DateTime.Now.Year.ToString();
                            string month = DateTime.Now.Month > 9 ? DateTime.Now.Month.ToString() : "0" + DateTime.Now.Month;
                            /*
                            path = ConstantHelper.UPLOAD_FOLDER + @"\" + year + @"\" + month;
                            path = HttpContext.Current.Server.MapPath(@"~\" + path);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            var createImageUniqueName = $"{tableID}_{recordID}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}.jpg";
                            bm.Save(path + @"\" + createImageUniqueName);
                            bm.Save(@"C:\uploads\" + createImageUniqueName);
                            */        
                            var createImageUniqueName = $"{tableID}_{recordID}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}.jpg";
                            var uploadPath = WebConfigurationManager.AppSettings["uploadPath"].ToString();
                            uploadPath += ConstantHelper.UPLOAD_FOLDER +  @"\" + year + @"\" + month; 
                            if (!Directory.Exists(uploadPath))
                                Directory.CreateDirectory(uploadPath);
                            bm.Save(uploadPath + @"\" + createImageUniqueName);

                            imageNameForSavingToDB = createImageUniqueName;
                            pathForSavingToDB = $"{ConstantHelper.UPLOAD_FOLDER}/{year}/{month}/{createImageUniqueName}";
                        }
                        var document = new sm_doc();
                        document.name = imageNameForSavingToDB;
                        document.tableID = tableID;
                        document.docs_CreatedDate = DateTime.Now;
                        document.value = recordID.ToString();
                        document.path = pathForSavingToDB;
                        db.sm_doc.Add(document);
                        await db.SaveChangesAsync();

                        documents.Add(document);
                    }
                }
            }
            return documents;
        }




        //-> keep this one, somehow will need it in the future
        //-> comment it dues to apply some logic
        //-> SaveUploadImage
        /*
        public static async Task<List<sm_doc>> SaveUploadImage(THEntities db, int tableID, int recordID, List<string> base64)
        {
            List<sm_doc> documents = new List<sm_doc>();
            if (base64 != null)
            {
                foreach (var image in base64)
                {
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(image)))
                    {
                        string pathForSavingToDB = "", imageNameForSavingToDB = "";
                        using (Bitmap bm = new Bitmap(ms))
                        {
                            string path = "";
                            string year = DateTime.Now.Year.ToString();
                            string month = DateTime.Now.Month > 9 ? DateTime.Now.Month.ToString() : "0" + DateTime.Now.Month;
                            path = ConstantHelper.UPLOAD_FOLDER + @"\" + year + @"\" + month;

                            path = HttpContext.Current.Server.MapPath(@"~\" + path);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            var createImageUniqueName = $"{tableID}_{recordID}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}.jpg";
                            bm.Save(path + @"\" + createImageUniqueName);
                            imageNameForSavingToDB = createImageUniqueName;
                            pathForSavingToDB = $"{ConstantHelper.UPLOAD_FOLDER}/{year}/{month}/{createImageUniqueName}";
                        }
                        var document = new sm_doc();
                        document.name = imageNameForSavingToDB;
                        document.tableID = tableID;
                        document.docs_CreatedDate = DateTime.Now;
                        document.value = recordID.ToString();
                        document.path = pathForSavingToDB;
                        db.sm_doc.Add(document);
                        await db.SaveChangesAsync();

                        documents.Add(document);
                    }
                }
            }
            return documents;
        }
        */

    }
}