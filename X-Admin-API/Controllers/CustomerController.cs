using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.Customer;
using X_Admin_API.Repository;

namespace X_Admin_API.Controllers
{
    public class CustomerController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "customers";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private CustomerRepository repository = null;
        
        public CustomerController()
        {
            repository = new CustomerRepository();
        }

        //-> Create New Customer
        [HttpPost]
        [Route(route)]
        [ResponseType(typeof(CustomerViewDTO))]
        public async Task<IHttpActionResult> Create(CustomerNewDTO customer)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(await repository.Create(customer));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Select Customer By ID
        [HttpGet]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(CustomerViewDTO))]
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

        //-> Edit Customer 
        [HttpPut]
        [Route(routeWithConstraint)]
        [ResponseType(typeof(CustomerViewDTO))]
        public async Task<IHttpActionResult> Edit(int id, [FromBody] CustomerEditDTO customer)
        {
            try
            {
                if (id != customer.id)
                    return BadRequest("Invalid id ");
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(await repository.Edit(customer));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //-> Delete Customer
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

        //-> Customer List
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<CustomerViewDTO>))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetList(currentPage));
        }

        //-> Search Customer
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<CustomerViewDTO>))]
        public async Task<IHttpActionResult> Search([FromUri] int currentPage, [FromUri] string search)
        {
            return Ok(await repository.Search(currentPage, search));
        }
    }
}
