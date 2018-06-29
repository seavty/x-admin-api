using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.Item;
using System.Threading.Tasks;
using X_Admin_API.Helper;
using X_Admin_API.Models.DTO.ItemGroup;
using System.Net;
using X_Admin_API.Models.DTO;

namespace X_Admin_API.Repository.Repo
{
    public class ItemRepository 
    {
        private THEntities db = null;

        public ItemRepository()
        {
            db = new THEntities();
        }

        //-> create
        public async Task<ItemViewDTO> Create(ItemNewDTO newDTO)
        {
            newDTO = StringHelper.TrimStringProperties(newDTO);
            var checkItemGroup = await db.tblItemGroups.FirstOrDefaultAsync(x => x.itmg_Deleted == null && x.id == newDTO.itemGroup.id); // check whether itemgroup name exist or not 
            if (checkItemGroup == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "Item group not exist");

            var checkItemCode = await db.tblItems.FirstOrDefaultAsync(r => r.item_Deleted == null && r.code == newDTO.code); // check whether itemgroup name exist or not
            if (checkItemCode != null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "This item code already exsits");

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrEmpty(newDTO.description))
                        newDTO.description = newDTO.name;
                    tblItem item = (tblItem)Helper.Helper.MapDTOToDBClass<ItemNewDTO, tblItem>(newDTO, new tblItem());
                    item.item_CreatedDate = DateTime.Now;
                    item.cost = 0;
                    item.quantity = 0;
                    item.lastCost = 0;

                    db.tblItems.Add(item);
                    await db.SaveChangesAsync();
                    List<sm_doc> documents = await Helper.Helper.SaveUploadImage(db, item.name, Helper.Helper.document_ItemTableID, item.id, newDTO.images);// tmp not useful , just reserve data for using in the furture
                    await SaveItemToWarehouses(item);
                    transaction.Commit();

                    return await SelectByID(item.id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        //-> SelectByID
        public async Task<ItemViewDTO> SelectByID(int id)
        {
            /*
            var items = await (from i in db.tblItems
                               join g in db.tblItemGroups.Where(x => x.itmg_Deleted == null)
                                    on i.itemGroupID equals g.id
                               join d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == Helper.Helper.document_ItemTableID)
                                    on i.id.ToString() equals d.value into document
                               where i.item_Deleted == null && i.id == id
                               orderby i.name ascending
                               select new { item = i, document = document, itemGroup = g }
                            ).ToListAsync();
             
            if (items.Count == 0)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");

            var itemGroupView = new ItemGroupBase();
            itemGroupView = Helper.Helper.MapDBClassToDTO<tblItemGroup, ItemGroupBase>(items[0].itemGroup);
            var itemView = new ItemViewDTO();
            itemView = Helper.Helper.MapDBClassToDTO<tblItem, ItemViewDTO>(items[0].item);
            itemView.documents = Helper.Helper.GetDocuments(items[0].document.ToList());
            itemView.itemGroup = itemGroupView;

            return itemView;
            */

            
            
             //--i want use like this, but seem getting error with ayn
            var item = await db.tblItems.FirstOrDefaultAsync(r => r.item_Deleted == null && r.id == id);
            if (item == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");

            var itemView = new ItemViewDTO();
            itemView = MappingHelper.MapDBClassToDTO<tblItem, ItemViewDTO>(item);
            itemView.documents = DocumentHelper.GetDocuments(db, ConstantHelper.document_ItemTableID, item.id);
            itemView.itemGroup = await new ItemGroupRepository().SelectByID(int.Parse(item.itemGroupID.ToString()));
            //itemView = MappingHelper.MapDBClassToDTO<tblItem, ItemViewDTO>(item); //if map at last like this , document & and item group will be null
            return itemView;
            
        }

        //-> Edit
        public async Task<ItemViewDTO> Edit(ItemEditDTO editDTO)
        {
            editDTO = StringHelper.TrimStringProperties(editDTO);
            tblItem item = db.tblItems.FirstOrDefault(r => r.item_Deleted == null && r.id == editDTO.id);
            if (item == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "This record has been deleted");

            var checkItemGroup = await db.tblItemGroups.FirstOrDefaultAsync(x => x.itmg_Deleted == null && x.id == editDTO.itemGroup.id); // check whether itemgroup name exist or not 
            if (checkItemGroup == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "Item group not exists");

            var checkItemCode = await db.tblItems.FirstOrDefaultAsync(r => r.item_Deleted == null && r.code == editDTO.code && r.id != editDTO.id);
            if (checkItemCode != null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "This item code already exsits");

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrEmpty(editDTO.description))
                        editDTO.description = editDTO.name;

                    item = (tblItem)Helper.Helper.MapDTOToDBClass<ItemEditDTO, tblItem>(editDTO, item);
                    item.item_UpdatedDate = DateTime.Now;
                    db.Entry(item).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //TODO : need to handle delete images or delete documents
                    List<sm_doc> documents = await Helper.Helper.SaveUploadImage(db, item.name, Helper.Helper.document_ItemTableID, item.id, editDTO.images);// tmp not useful , just reserve data for using in the furture
                    transaction.Commit();

                    return await SelectByID(item.id);
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
            var item = await db.tblItems.FirstOrDefaultAsync(x => x.item_Deleted == null && x.id == id);
            if (item == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            item.item_Deleted = "1";
            await db.SaveChangesAsync();
            return true;
        }

        //-> GetList
        public async Task<GetListDTO<ItemViewDTO>> GetList(int currentPage)
        {
            IQueryable<tblItem> items = from i in db.tblItems
                                        where i.item_Deleted == null
                                        orderby i.name ascending
                                        select i;
            return await Listing(currentPage, items);
        }

        //-> Search
        public async Task<GetListDTO<ItemViewDTO>> Search(int currentPage, string search)
        {
            IQueryable<tblItem> items = from i in db.tblItems
                                        where i.item_Deleted == null && (i.name.Contains(search) || i.code.Contains(search))
                                        orderby i.name ascending
                                        select i;
            return await Listing(currentPage, items, search);
        }

        //-> uploadimages
        public async Task<ItemViewDTO> UploadImages(ItemUploadImageDTO itemUploadImage)
        {
            tblItem item = db.tblItems.FirstOrDefault(r => r.item_Deleted == null && r.id == itemUploadImage.id);
            if (item == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "This record does not exsists or has been deleted");
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<sm_doc> documents = await DocumentHelper.SaveUploadImage(db, ConstantHelper.document_ItemTableID, itemUploadImage.id, itemUploadImage.base64s);
                    transaction.Commit();

                    return await SelectByID(item.id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        //*** private method ***/
        private async Task<GetListDTO<ItemViewDTO>> Listing(int currentPage, IQueryable<tblItem> items, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage);
            var myList = new List<ItemViewDTO>();
            var myRecords = items.Skip(startRow).Take(Helper.Helper.pageSize);
            foreach (var item in myRecords)
            {
                myList.Add(await SelectByID(item.id));
            }
            var getList = new GetListDTO<ItemViewDTO>();
            getList.metaData = await Helper.Helper.GetMetaData(currentPage, items, "name", "asc", search);
            getList.results = myList;
            return getList;
        }

        private async Task SaveItemToWarehouses(tblItem item)
        {
            var wareHouses = await db.tblWarehouses.Where(r => r.ware_Deleted == null).ToListAsync();
            foreach (var wareHouse in wareHouses)
            {
                var itemWareHouse = new tblItemWarehouse();
                itemWareHouse.itemID = item.id;
                itemWareHouse.wareHouseID = wareHouse.id;
                itemWareHouse.quantity = 0;
                itemWareHouse.itwh_CreatedDate = DateTime.Now;
                db.tblItemWarehouses.Add(itemWareHouse);
                db.SaveChanges();
            }
        }
    }
}