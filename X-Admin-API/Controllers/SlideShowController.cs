using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO.SlideShow;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class SlideShowController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "slideshows";
        private const string routeWithConstraint = route + "/{id:int:min(-1)}";
        private SlideShowRepository repository = null;

        public SlideShowController()
        {
            repository = new SlideShowRepository();
        }

        //-> Create New SlideShow
        [HttpPost]
        [Route(route)]
        [ResponseType(typeof(SlideShowListDTO))]
        public async Task<IHttpActionResult> Create(SlideShowNewDTO slideShow)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await repository.Create(slideShow));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Select SlideShow By ID
        [HttpGet]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(SlideShowListDTO))]
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
    }
}
