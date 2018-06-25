using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.Warehouse;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class WarehouseController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "warehouses";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private WarehouseRepository repository = null;

        public WarehouseController()
        {
            repository = new WarehouseRepository();
        }

        //-> Warehouse List
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(GetListDTO<WarehouseViewDTO>))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetList(currentPage));
        }
    }
}
