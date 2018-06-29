using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.SaleOrder;


namespace X_Admin_API.Repository.Repo
{
    public class SaleOrderRepository 
    {
        private THEntities db = null;

        public SaleOrderRepository()
        {
            db = new THEntities();
        }


        //-> SelectByID
        public async Task<SaleOrderViewDTO> SelectByID(int id)
        {
            var saleOrder = await db.tblSaleOrders.FirstOrDefaultAsync(s => s.sord_Deleted == null && s.id == id);
            if (saleOrder == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");

            var saleOrderView = Helper.Helper.MapDBClassToDTO<tblSaleOrder, SaleOrderViewDTO>(saleOrder);
            saleOrderView.customer = await (new CustomerRepository()).SelectByID(int.Parse(saleOrder.customerID.ToString()));
            return saleOrderView;
        }

        public async Task<SaleOrderListDTO> GetList(int currentPage)
        {
            IQueryable<tblSaleOrder> saleOrders = from s in db.tblSaleOrders
                                                where s.sord_Deleted == null
                                                orderby s.saleOrderNo ascending
                                                select s;
            return await Listing(currentPage, saleOrders);
        }


        //-> Delete
        public async Task<Boolean> Delete(int id)
        {
            var saleOrder = await db.tblSaleOrders.FirstOrDefaultAsync(r => r.sord_Deleted == null && r.id == id);
            if (saleOrder == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            saleOrder.sord_Deleted = "1";
            await db.SaveChangesAsync();
            return true;
        }


        //-- private method --//
        private async Task<SaleOrderListDTO> Listing(int currentPage, IQueryable<tblSaleOrder> saleOrders, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var saleOrderViewList = new List<SaleOrderViewDTO>();
            var mySaleOrders = await saleOrders.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            foreach (var saleOrder in mySaleOrders)
            {
                saleOrderViewList.Add(await SelectByID(saleOrder.id));
            }
            var saleOrderList = new SaleOrderListDTO();
            saleOrderList.metaData = await Helper.Helper.GetMetaData(currentPage, saleOrders, "name", "asc", search);
            saleOrderList.results = saleOrderViewList;
            return saleOrderList;
        }
    }
}