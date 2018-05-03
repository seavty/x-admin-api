using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Helper;
using X_Admin_API.Models.DTO.Invoice;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class invoiceController : ApiController
    {
        private invoiceRepo repository = null;
        public invoiceController()
        {
            repository = new invoiceRepo();
        }
        [HttpPost]
        //[Route(route)]
        [ResponseType(typeof(invoiceDetailListDTO))]
        public async Task<IHttpActionResult> Create(invoiceDetailListDTO newDTO)
        {
            sapi.db db = new sapi.db();
            try
            {
                db.connect();
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string token = "";
                System.Net.Http.Headers.HttpRequestHeaders headers = this.Request.Headers;
                if (headers.Contains("token"))
                {
                    foreach (var s in headers.GetValues("token"))
                    {
                        token = s;
                    }
                }
                return Ok(await xcrm.UploadInvoice(db, newDTO, token));
                //return Ok(await repository.Create(saleOrderItem, token));
            }
            catch (HttpException ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                db.close();
            }
        }

        [HttpGet]
        [ResponseType(typeof(invoiceBase))]
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

        [HttpGet]
        [ResponseType(typeof(invoiceListDTO))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetList(currentPage));
        }

        [HttpGet]

        [ResponseType(typeof(invoiceDetailDTO))]
        public async Task<IHttpActionResult> GetDetail(int id, [FromUri] int currentPage, [FromUri] string search)
        {
            string token = "";
            System.Net.Http.Headers.HttpRequestHeaders headers = this.Request.Headers;
            if (headers.Contains("token"))
            {
                foreach (var s in headers.GetValues("token"))
                {
                    token = s;
                }
            }
            return Ok(await repository.GetList(id, currentPage, token, search));
        }
    }
}
