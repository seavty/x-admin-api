using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.Warehouse;

namespace X_Admin_API.Repository.Repo
{
    public class WarehouseRepository
    {
        private THEntities db = null;

        public WarehouseRepository()
        {
            db = new THEntities();
        }

        //-> SelectByID
        public async Task<WarehouseViewDTO> SelectByID(int id)
        {
            var record = await db.tblWarehouses.FirstOrDefaultAsync(x => x.ware_Deleted == null && x.id == id);
            if (record == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            return Helper.Helper.MapDBClassToDTO<tblWarehouse, WarehouseViewDTO>(record);
        }

        //-> GetList
        public async Task<GetListDTO<WarehouseViewDTO>> GetList(int currentPage)
        {
            IQueryable<tblWarehouse> records = from x in db.tblWarehouses
                                                where x.ware_Deleted == null
                                                orderby x.name ascending
                                                select x;
            return await Listing(currentPage, records);
        }

        //-- private method --//
        private async Task<GetListDTO<WarehouseViewDTO>> Listing(int currentPage, IQueryable<tblWarehouse> records, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var myList = new List<WarehouseViewDTO>();
            var myRecords = await records.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            foreach (var record in myRecords)
            {
                myList.Add(await SelectByID(record.id));
            }
            var getList = new GetListDTO<WarehouseViewDTO>();
            getList.metaData = await Helper.Helper.GetMetaData(currentPage, records, "name", "asc", search);
            getList.results = myList;
            return getList;
        }
    }
}