using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using X_Admin_API.Models;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.Document;
using X_Admin_API.Models.DTO.User;

namespace X_Admin_API.Helper
{
    public static class Helper
    {
        public const string apiVersion = "api/v1/";
        public static readonly int pageSize = 20;
        public static readonly int pageSize_MasterDetailList = 10;

        public static readonly int document_ItemTableID = 15;
        public static readonly int document_ItemGroupTableID = 16;

        public static readonly int document_SlideShowTableID = -1;
        public static readonly int slideShow_RecordID = -1;
        public static readonly string slideShowDocumentName = "SlideShow";

        public static bool IsValidToken(string token)
        {
            using (THEntities db = new THEntities())
            {
                string encryptToken = EncryptString(token);
                return db.sys_user.Any(r => r.user_Deleted == null && r.token == encryptToken);
            }
        }

        public static UserProfileViewDTO GetUserProfile(string token)
        {
            using (THEntities db = new THEntities())
            {
                string encryptToken = EncryptString(token);
                var user = db.sys_user.FirstOrDefault(x => x.user_Deleted == null && x.token == encryptToken);
                UserProfileViewDTO userProfile = MapDBClassToDTO<sys_user, UserProfileViewDTO>(user);
                return userProfile;
            }
        }

        public static T GetQueryString<T>()
        {
            T result = default(T);
            return result;

        }
        public static int GetStartRow(int currentPage, bool isMasterDetailList=false)
        {
            int startRow = 0;
            if (currentPage > 1)
            {
                int myPageSize = pageSize;
                if (isMasterDetailList)
                    myPageSize = pageSize_MasterDetailList;
                startRow = (currentPage - 1) * myPageSize;
            }
            return startRow;
        }
        
        public static async Task<MetaData> GetMetaData(int currentPage, IQueryable<dynamic> records, string orderColumn, string orderBy, string search, bool isMasterDetailList=false)
        {
            int myPageSize = pageSize;
            if (isMasterDetailList)
                myPageSize = pageSize_MasterDetailList;

            int totalRecord = await records.CountAsync();
            double getTotalPage = ((double)totalRecord / myPageSize);
            int totalPage = (int)Math.Ceiling(getTotalPage);
            MetaData metaData = new MetaData();
            metaData.currentPage = currentPage;
            metaData.pageSize = myPageSize;
            metaData.totalPage = totalPage;
            metaData.totalRecord = totalRecord;
            metaData.orderColumn = orderColumn;
            metaData.orderBy = orderBy;
            metaData.search = search;
            return metaData;
        }

        //-> encrypt string
        public static string EncryptString(string pwd)
        {
            CryptLib _crypt = new CryptLib();
            string plainText = pwd;
            string iv = "Xsoft";// CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
            string key = CryptLib.getHashSha256("@XSoft201701", 31); //32 bytes = 256 bits

            return _crypt.encrypt(plainText, key, iv);

        }


        //-> DBClass To DTO
        public static DTO MapDBClassToDTO<DBclass, DTO>(Object record)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DBclass, DTO>());
            var mapper = config.CreateMapper();
            DTO dto = mapper.Map<DTO>(record);
            return dto;
        }

        //-> DTO To DBClass
        public static Object MapDTOToDBClass<DTO, DBclass>(Object dto, Object record)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DTO, DBclass>());
            var mapper = config.CreateMapper();
            record = mapper.Map(dto, record);
            return record;
        }

        //-> Get Document
        public static List<DocumentViewDTO> GetDocuments(List<sm_doc> smDoc)
        {
            List<DocumentViewDTO> documentViews = new List<DocumentViewDTO>();
            foreach (var document in smDoc)
            {
                //documentViews.Add(MapDBClassToDTO<sm_doc, DocumentViewDTO>(document));
                document.path = WebConfigurationManager.AppSettings["baseURL"].ToString() + document.path;
                documentViews.Add(MapDBClassToDTO<sm_doc, DocumentViewDTO>(document));
            }
            return documentViews;
        }
        
        //-- Save upload document --//
        public static async Task<List<sm_doc>> SaveUploadImage(THEntities db, string documentName, int tableID, int recordID , List<string> base64)
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
                            string uploadFolderName = "uploads";
                            path = HttpContext.Current.Server.MapPath(@"~\" + uploadFolderName);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            string currentYear = DateTime.Now.Year.ToString();
                            path += @"\" + currentYear;
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            string currentMonth = DateTime.Now.Month > 9 ? DateTime.Now.Month.ToString() : "0" + DateTime.Now.Month;
                            path += @"\" + currentMonth;

                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            var createImageUniqueName = recordID + "_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmssfff") + ".jpg";
                            bm.Save(path + @"\" + createImageUniqueName);

                            imageNameForSavingToDB = documentName + "_" + createImageUniqueName;
                            pathForSavingToDB = uploadFolderName + "/" + currentYear + "/" + currentMonth + "/" + createImageUniqueName;
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
    }
}