using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Helper;
using X_Admin_API.Models.DTO;
using X_Admin_API.Models.DTO.User;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    
    public class UserController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "users";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private UserRepository repository = null;

        public UserController()
        {
            repository = new UserRepository();
        }

        //-> Login
        [SkipAuthentication]
        [HttpPost]
        [Route(route)]
        [ResponseType(typeof(UserProfileViewHasToken))]
        public async Task<IHttpActionResult> Login(UserCrendential credential)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await repository.Login(credential);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
