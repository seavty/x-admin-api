using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.Item;
using X_Admin_API.Repository.Repo;
using X_Admin_API.Utils.Attribute;

namespace X_Admin_API.Controllers
{
    [ErrorLoggerAttribute]
    public class ItemController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "items";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private const string uploadImages = routeWithConstraint + "/uploadimages";
        private ItemRepository repository = null;

        public ItemController()
        {
            repository = new ItemRepository();
        }
        
        //-> Create New Item 
        [HttpPost]
        [Route(route)]
        [ResponseType(typeof(ItemViewDTO))]
        public async Task<IHttpActionResult> Create(ItemNewDTO item)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await repository.Create(item));
            }
            catch(HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Select Item By ID 
        [HttpGet]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(ItemViewDTO))]
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

        //-> Edit Item
        [HttpPut]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(ItemViewDTO))]
        public async Task<IHttpActionResult> Edit(int id, [FromBody] ItemEditDTO item)
        {
            try
            {
                if (id != item.id)
                    return BadRequest("Invalid id ");
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(await repository.Edit(item));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Delete Item
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
            catch (HttpException)
            {
                return NotFound();
            }
        }

        //-> Item List
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<ItemViewDTO>))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetList(currentPage));
        }

        //-> Search Item
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<ItemViewDTO>))]
        public async Task<IHttpActionResult> Search([FromUri] int currentPage, [FromUri] string search)
        {
            return Ok(await repository.Search(currentPage, search));
        }

        //-> upload image
        [HttpPost]
        [Route(uploadImages)]
        [ResponseType(typeof(ItemViewDTO))]
        public async Task<IHttpActionResult> UploadImages(int id, [FromBody] ItemUploadImageDTO item)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await repository.UploadImages(item));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
