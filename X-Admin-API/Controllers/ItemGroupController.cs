using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.Item;
using X_Admin_API.Models.DTO.ItemGroup;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class ItemGroupController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "itemgroups";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private const string routeWithConstraintForGetDetailForMaster = routeWithConstraint + "/items";
        private const string uploadImages = routeWithConstraint + "/uploadimages";

        private ItemGroupRepository repository = null;

        public ItemGroupController()
        {
            repository = new ItemGroupRepository();
        }

        //-> Create New ItemgGroup 
        [HttpPost]
        [Route(route)]
        [ResponseType(typeof(ItemGroupViewDTO))]
        public async Task<IHttpActionResult> Create(ItemGroupNewDTO itemGroup)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await repository.Create(itemGroup));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Select ItemGroup By ID 
        [HttpGet]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(ItemGroupViewDTO))]
        public async Task<IHttpActionResult> SelectByID(int id)
        {
            try
            {
                return Ok(await repository.SelectByID(id));
            }
            catch (HttpException)
            {
                return NotFound();
            }
        }

        //-> Edit ItemGroup
        [HttpPut]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(ItemGroupViewDTO))]
        public async Task<IHttpActionResult> Edit(int id, [FromBody] ItemGroupEditDTO itemGroup)
        {
            try
            {
                if (id != itemGroup.id)
                    return BadRequest("Invalid id ");
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(await repository.Edit(itemGroup));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Delete ItemGroup
        [HttpDelete]
        [Route(routeWithConstraint)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
                if (await repository.Delete(id))
                    return Ok();
                return NotFound();
            }
            catch (HttpException ex)
            {
                if (ex.Message == "Not Found")
                    return NotFound();
                else
                    return BadRequest(ex.Message);
            }
            
        }

        //-> ItemGroup List
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<ItemGroupViewDTO>))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetList(currentPage));
        }

        //-> Search ItemGroup
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<ItemGroupViewDTO>))]
        public async Task<IHttpActionResult> Search([FromUri] int currentPage, [FromUri] string search)
        {
            return Ok(await repository.Search(currentPage, search));
        }


        //** item list for item group **//

        //-> get items for itemGrup
        [HttpGet]
        [Route(routeWithConstraintForGetDetailForMaster)]
        [ResponseType(typeof(GetListDTO<ItemViewForMasterDTO>))]
        public async Task<IHttpActionResult> GetItemsByItemGroup(int id, [FromUri] int currentPage)
        {
            var items = await repository.GetDetailForMaster(id, currentPage);
            if (items == null)
                return NotFound();
            return Ok(items);
        }

        //-> Search Item for item group
        [HttpGet]
        [Route(routeWithConstraintForGetDetailForMaster)]
        [ResponseType(typeof(GetListDTO<ItemViewForMasterDTO>))]
        public async Task<IHttpActionResult> SearchItemsByItemGroup(int id, [FromUri] int currentPage, [FromUri] string search)
        {
            return Ok(await repository.GetDetailForMasterSearch(id, currentPage, search));
        }


        //-> upload image
        [HttpPost]
        [Route(uploadImages)]
        [ResponseType(typeof(ItemGroupViewDTO))]
        public async Task<IHttpActionResult> UploadImages(int id, [FromBody] ItemGroupUploadImageDTO itemGroup)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await repository.UploadImages(itemGroup));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}