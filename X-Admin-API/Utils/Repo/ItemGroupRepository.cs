using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.ItemGroup;
using System.Threading.Tasks;
using X_Admin_API.Helper;
using X_Admin_API.Models.DTO.Item;
using System.Web;
using System.Net;
using X_Admin_API.Models.DTO;

namespace X_Admin_API.Repository.Repo
{
    public class ItemGroupRepository 
    {
        private THEntities db = null;

        public ItemGroupRepository()
        {
            db = new THEntities();
        }

        //-> Create
        public async Task<ItemGroupViewDTO> Create(ItemGroupNewDTO newDTO)
        {
            newDTO = StringHelper.TrimStringProperties(newDTO);
            var checkName = await db.tblItemGroups.FirstOrDefaultAsync(r => r.itmg_Deleted == null && r.name == newDTO.name); // check whether itemgroup name exist or not
            if (checkName != null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "This name already exsits");

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    tblItemGroup itemGroup = (tblItemGroup)Helper.Helper.MapDTOToDBClass<ItemGroupNewDTO, tblItemGroup>(newDTO, new tblItemGroup());
                    itemGroup.itmg_CreatedDate = DateTime.Now;
                    db.tblItemGroups.Add(itemGroup);
                    await db.SaveChangesAsync();
                    List<sm_doc> documents = await Helper.Helper.SaveUploadImage(db, itemGroup.name, Helper.Helper.document_ItemGroupTableID, itemGroup.id, newDTO.images);// tmp not useful , just reserve data for using in the furture
                    transaction.Commit();
                    return await SelectByID(itemGroup.id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        //-> SelectByID
        public async Task<ItemGroupViewDTO> SelectByID(int id)
        {
            var itemGroups = await (from g in db.tblItemGroups
                                    join d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == Helper.Helper.document_ItemGroupTableID)
                                    on g.id.ToString() equals d.value into document
                                    where g.itmg_Deleted == null && g.id == id
                                    orderby g.name ascending
                                    select new { itemGroup = g, document = document }
                            ).ToListAsync();
            if (itemGroups.Count == 0)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");

            var itemGroupView = new ItemGroupViewDTO();
            itemGroupView = Helper.Helper.MapDBClassToDTO<tblItemGroup, ItemGroupViewDTO>(itemGroups[0].itemGroup);
            itemGroupView.documents = Helper.Helper.GetDocuments(itemGroups[0].document.ToList());
            
            return itemGroupView;
        }

        //-> Edit
        public async Task<ItemGroupViewDTO> Edit(ItemGroupEditDTO editDTO)
        {
            //TODO -- when edit -> //user can add new image or delete current images i mean documents 
            editDTO = StringHelper.TrimStringProperties(editDTO);
            tblItemGroup itemGroup = db.tblItemGroups.FirstOrDefault(r => r.itmg_Deleted == null && r.id == editDTO.id);
            if (itemGroup == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "This record has been deleted");

            var checkName = await db.tblItemGroups.FirstOrDefaultAsync(r => r.itmg_Deleted == null && r.name == editDTO.name && r.id != editDTO.id);
            if (checkName != null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "This name already exsits");

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    itemGroup.itmg_UpdatedDate = DateTime.Now;
                    itemGroup = (tblItemGroup)Helper.Helper.MapDTOToDBClass<ItemGroupEditDTO, tblItemGroup>(editDTO, itemGroup);
                    db.Entry(itemGroup).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    List<sm_doc> documents = await Helper.Helper.SaveUploadImage(db, itemGroup.name, Helper.Helper.document_ItemGroupTableID, itemGroup.id, editDTO.images);// tmp not useful , just reserve data for using in the furture
                    transaction.Commit();
                    return await SelectByID(itemGroup.id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        //-> Delete
        public async Task<bool> Delete(int id)
        {
            var itemGroup = await db.tblItemGroups.FirstOrDefaultAsync(r => r.itmg_Deleted == null && r.id == id);
            if (itemGroup == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "Not Found");

            IQueryable<tblItem> items = from i in db.tblItems
                                        where i.item_Deleted == null && i.itemGroupID == id
                                        orderby i.name ascending
                                        select i;
            int totalRecord = await items.CountAsync();
            if (totalRecord>0)
                throw new HttpException((int)HttpStatusCode.BadRequest, "Cannot delete this record because it is currently in use!");

            itemGroup.itmg_Deleted = "1";
            await db.SaveChangesAsync();
            return true;
        }

        //-> GetList
        public async Task<GetListDTO<ItemGroupViewDTO>> GetList(int currentPage)
        {
            /*
            IQueryable<tblItemGroup> itemGroups = from g in db.tblItemGroups
                                                  where g.itmg_Deleted == null
                                                  orderby g.name ascending
                                                  select g;
            */
            IQueryable<tblItemGroup> itemGroups = from g in db.tblItemGroups
                                                  where g.itmg_Deleted == null
                                                  orderby g.id ascending
                                                  select g;
            return await Listing(currentPage, itemGroups);
        }

        //-> Search
        public async Task<GetListDTO<ItemGroupViewDTO>> Search(int currentPage, string search)
        {
            IQueryable<tblItemGroup> itemGroups = from g in db.tblItemGroups
                                                  where g.itmg_Deleted == null && g.name.Contains(search)
                                                  orderby g.name ascending
                                                  select g;
            return await Listing(currentPage, itemGroups, search);
        }


        //-- item list for master
        public async Task<GetListDTO<ItemViewForMasterDTO>> GetDetailForMaster(int masterID, int currentPage)
        {
            var items = from i in db.tblItems
                        join d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == Helper.Helper.document_ItemTableID)
                        on i.id.ToString() equals d.value into document
                        where i.item_Deleted == null && i.itemGroupID == masterID
                        orderby i.name ascending
                        select new { item = i, document = document };
            return await ListingForMasterDetail(currentPage, items);
        }

        public async Task<GetListDTO<ItemViewForMasterDTO>> GetDetailForMasterSearch(int masterID, int currentPage, string search)
        {
            var items = from i in db.tblItems
                        join d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == Helper.Helper.document_ItemTableID)
                        on i.id.ToString() equals d.value into document
                        where i.item_Deleted == null && i.itemGroupID == masterID && i.name.Contains(search)
                        orderby i.name ascending
                        select new { item = i, document = document };
            return await ListingForMasterDetail(currentPage, items, search);
            
        }


        //-> uploadimages
        public async Task<ItemGroupViewDTO> UploadImages(ItemGroupUploadImageDTO itemGroupUploadImage)
        {
            tblItemGroup itemGroup = db.tblItemGroups.FirstOrDefault(r => r.itmg_Deleted == null && r.id == itemGroupUploadImage.id);
            if (itemGroup == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "This record does not exsists or has been deleted");
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<sm_doc> documents = await DocumentHelper.SaveUploadImage(db, ConstantHelper.document_ItemGroupTableID, itemGroupUploadImage.id, itemGroupUploadImage.base64s);
                    transaction.Commit();

                    return await SelectByID(itemGroup.id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }



        //-- private function --//

        //-> ListingForMasterDetail
        private async Task<GetListDTO<ItemViewForMasterDTO>> ListingForMasterDetail(int currentPage, IQueryable<dynamic> items, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            List<ItemViewForMasterDTO> itemViewForMasters = new List<ItemViewForMasterDTO>();
            var myItems = await items.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            foreach (var item in myItems)
            {
                var itemViewForMaster = new ItemViewForMasterDTO();
                itemViewForMaster = Helper.Helper.MapDBClassToDTO<tblItem, ItemViewForMasterDTO>(item.item);
                itemViewForMaster.documents = Helper.Helper.GetDocuments(item.document);
                itemViewForMasters.Add(itemViewForMaster);
            }
            var getList = new GetListDTO<ItemViewForMasterDTO>();
            getList.metaData = await Helper.Helper.GetMetaData(currentPage, items, "name", "asc", search);
            getList.results = itemViewForMasters;
            return getList;
        }

        //-> Listing
        private async Task<GetListDTO<ItemGroupViewDTO>> Listing(int currentPage, IQueryable<tblItemGroup> itemGroups, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var myList = new List<ItemGroupViewDTO>();
            var myItemGroups = await itemGroups.Skip(startRow).Take(Helper.Helper.pageSize).ToListAsync();
            foreach (var itemGroup in myItemGroups)
            {
                myList.Add(await SelectByID(itemGroup.id));
            }
            var getList = new GetListDTO<ItemGroupViewDTO>();
            getList.metaData = await Helper.Helper.GetMetaData(currentPage, itemGroups, "name", "asc", search);
            getList.results = myList;
            return getList;
        }
    }
}