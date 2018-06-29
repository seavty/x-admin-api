using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.ItemGroupWithItem;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class ItemGroupWithItemController : ApiController
    {

        private const string route = Helper.Helper.apiVersion + "itemgroupwithitems";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private ItemGroupWithItemRepository repository = null;

        public ItemGroupWithItemController()
        {
            repository = new ItemGroupWithItemRepository();
        }

        //-> ItemGroupWithItem List
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<ItemGroupWithItemViewDTO>))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetMasterDetailList(currentPage));
        }


        ////--- Get -- //
        //[HttpGet]
        //[Route("api/v1/itemgroupwithitems")]
        //[ResponseType(typeof(ItemGroupWithItemResult))]
        //public IHttpActionResult Get([FromUri] int currentPage)
        //{
        //    return Ok(repository.Get(currentPage));
        //}
    }
}
