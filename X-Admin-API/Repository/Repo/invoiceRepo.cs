using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.Invoice;

namespace X_Admin_API.Repository.Repo
{
    public class invoiceRepo
    {
        private THEntities db = null;
        public invoiceRepo()
        {
            db = new THEntities();
        }

        //-> SelectByID
        public async Task<invoiceBase> SelectByID(int id)
        {
           
            var tbl = await db.vInvoices.FirstOrDefaultAsync(r => r.invo_Deleted == null && r.invo_InvoiceID == id);
            if (tbl == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            return Helper.Helper.MapDBClassToDTO<tblInvoice, invoiceBase>(tbl);
        }

        //-> GetList
        public async Task<invoiceListDTO> GetList(int currentPage, string token = "")
        {
            IQueryable<vInvoice> tbl = null;
            if (!string.IsNullOrEmpty(token))
            {
                var users = Helper.Helper.GetUserProfile(token);
                if (users == null)
                {
                    throw new HttpException((int)HttpStatusCode.Unauthorized, "Invalid Token !");
                }
                tbl = from r in db.vInvoices
                      where r.invo_Deleted == null
                      orderby r.invo_InvoiceID descending
                      select r;
            }
            else
            {

                tbl = from r in db.vInvoices
                      where r.invo_Deleted == null
                      orderby r.invo_InvoiceID descending
                      select r;
            }
            return await Listing(currentPage, tbl);
        }

        private async Task<invoiceListDTO> Listing(int currentPage, IQueryable<vInvoice> tbls, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var tblList = new List<invoiceBase>();
            var _tbls = await tbls.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            invoiceListDTO _tblList = new invoiceListDTO();
            foreach (var tbl in _tbls)
            {
                tblList.Add(await SelectByID(tbl.invo_InvoiceID));
            }

            _tblList.metaData = await Helper.Helper.GetMetaData(currentPage, tbls, "invo_InvoiceID", "asc", search);
            _tblList.results = tblList;
            return _tblList;
        }

        //

        public async Task<invoiceDetailDTO> GetList(int id, int currentPage, string token, string search)
        {
            var users =Helper.Helper.GetUserProfile(token);
            if (users == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Invalid Token !");
            }
            sys_setting setting = await db.sys_setting.FirstOrDefaultAsync(r => r.id == r.id);
            IQueryable<vInvoice> tbl = from r in db.vInvoices
                                       where r.invo_Deleted == null
                                       && (string.IsNullOrEmpty(search) ? 1 == 1 :
                                          (r.invo_Name.Contains(search) || r.cust_Name.Contains(search))
                                       )
                                       &&
                                      (setting.sett_useSalesman.ToString().ToLower() == "y" ?
                                          r.invo_SalesmanID == users.user_SalesmanID :
                                          r.invo_CreatedBy == users.id
                                      )
                                       orderby r.invo_Name descending
                                       select r;
            return await ListingDetail(currentPage, tbl);
        }

        private async Task<invoiceDetailDTO> ListingDetail(int currentPage, IQueryable<vInvoice> tbls, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var _tbls = await tbls.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            invoiceDetailDTO _tblList = new invoiceDetailDTO();
            List<invoiceDetailListDTO> _tblSubs = new List<invoiceDetailListDTO>();
            foreach (var tbl in _tbls)
            {
                invoiceDetailListDTO tmp = new invoiceDetailListDTO();
                tmp = Helper.Helper.MapDBClassToDTO<vInvoice, invoiceDetailListDTO>(tbl);
                var tblSubs = await db.tblInvoiceItems.Where(r => r.init_InvoiceID == tbl.invo_InvoiceID).ToListAsync();
                List<invoiceItemBase> tblItem = new List<invoiceItemBase>();
                foreach (var tblSub in tblSubs)
                {
                    tblItem.Add(Helper.Helper.MapDBClassToDTO<tblInvoiceItem, invoiceItemBase>(tblSub));
                }
                tmp.results = tblItem;
                _tblSubs.Add(tmp);
            }
            _tblList.results = _tblSubs;
            _tblList.metaData = await Helper.Helper.GetMetaData(currentPage, tbls, "invo_Name", "asc", search);
            return _tblList;
        }
    }
}