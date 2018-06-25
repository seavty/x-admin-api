using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class DocumentController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "documents";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private DocumentRepository repository = null;

        public DocumentController()
        {
            repository = new DocumentRepository();
        }

        //-> Delete Document
        [Route(routeWithConstraint)]
        [HttpDelete]
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
    }
}
