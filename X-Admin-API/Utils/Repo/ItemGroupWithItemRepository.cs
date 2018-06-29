using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.Item;
using X_Admin_API.Models.DTO.ItemGroupWithItem;

namespace X_Admin_API.Repository.Repo
{
    public class ItemGroupWithItemRepository
    {
        private THEntities db = null;

        public ItemGroupWithItemRepository()
        {
            db = new THEntities();
        }

        //-> GetMasterDetailList
        public async Task<GetListDTO<ItemGroupWithItemViewDTO>> GetMasterDetailList(int currentPage)
        {
            var itemGroups = from g in db.tblItemGroups
                             join d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == Helper.Helper.document_ItemGroupTableID)
                             on g.id.ToString() equals d.value into document
                             where g.itmg_Deleted == null
                             orderby g.name ascending
                             select new { itemGroup = g, document = document };
            return await Listing(currentPage, itemGroups);

        }

        //-- ** private method --**/
        //-> Listing
        private async Task<GetListDTO<ItemGroupWithItemViewDTO>> Listing(int currentPage, IQueryable<dynamic> itemGroups, string search = null)
        {
            int startRow = Helper.Helper.GetStartRow(currentPage, true);
            List<ItemGroupWithItemViewDTO> itemGroupWithItemViews = new List<ItemGroupWithItemViewDTO>();
            var myItemGroups = itemGroups.Skip(startRow).Take(Helper.Helper.pageSize_MasterDetailList);
            foreach (var itemGroup in myItemGroups )
            {
                var itemGroupWithItem = new ItemGroupWithItemViewDTO();
                itemGroupWithItem = Helper.Helper.MapDBClassToDTO<tblItemGroup, ItemGroupWithItemViewDTO>(itemGroup.itemGroup);
                itemGroupWithItem.items = await GetItemsForGroup(itemGroup.itemGroup.id);
                itemGroupWithItem.documents = Helper.Helper.GetDocuments(itemGroup.document);
                itemGroupWithItemViews.Add(itemGroupWithItem);
            }
            var getList = new GetListDTO<ItemGroupWithItemViewDTO>();
            getList.metaData = await Helper.Helper.GetMetaData(currentPage, itemGroups, "name", "asc", search, true);
            getList.results = itemGroupWithItemViews;
            return getList;
        }


        //-> GetItemsForGroup
        private async Task<List<ItemViewForMasterDTO>> GetItemsForGroup(int itemGroupID)
        {
            var items = from i in db.tblItems
                        join d in db.sm_doc.Where(x => x.docs_Deleted == null && x.tableID == Helper.Helper.document_ItemTableID)
                        on i.id.ToString() equals d.value into document
                        where i.item_Deleted == null && i.itemGroupID == itemGroupID
                        orderby i.name ascending
                        select new { item = i, document = document };
                          
            List<ItemViewForMasterDTO> itemViewForMasters = new List<ItemViewForMasterDTO>();
            var myItem = await items.Take(Helper.Helper.pageSize).ToListAsync(); // here no skip record, becos we want to take 20 tblItems for every single Item group;
            foreach (var item in items.Take(Helper.Helper.pageSize))
            {
                var itemViewForMaster = new ItemViewForMasterDTO();
                itemViewForMaster = Helper.Helper.MapDBClassToDTO<tblItem, ItemViewForMasterDTO>(item.item);
                itemViewForMaster.documents = Helper.Helper.GetDocuments(item.document.ToList());
                itemViewForMasters.Add(itemViewForMaster);
            }
            return itemViewForMasters;
        }
    }
}