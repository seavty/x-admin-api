using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using X_Admin_API.Models.DTO.SaleOrder;
using X_Admin_API.Models.DTO.User;
using X_Admin_API.Repository.Repo;

namespace X_Admin_API.Controllers
{
    public class SaleOrderController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "saleorders";
        private const string routeWithConstraint = route + "/{id:int:min(1)}";
        private SaleOrderRepository repository = null;

        public SaleOrderController()
        {
            repository = new SaleOrderRepository();
        }

        //-> Customer List
        [HttpGet]
        [Route(route)]
        [ResponseType(typeof(SaleOrderListDTO))]
        public async Task<IHttpActionResult> Get([FromUri] int currentPage)
        {
            return Ok(await repository.GetList(currentPage));
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
            catch (HttpException)
            {
                return NotFound();
            }
        }


        [HttpPost]
        [Route(route)]
        public IHttpActionResult SaleOrder(SaleOrderNewDTO saleOrder)
        {
            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    vals.Add("sord_SaleOrderID".ToLower(), "");
                    vals.Add("sord_Date".ToLower(), DateTime.Now.ToString("dd/MM/yyyy"));
                    vals.Add("sord_CustomerID".ToLower(), saleOrder.customerID.ToString());
                    vals.Add("sord_WarehouseId".ToLower(), saleOrder.warehouseID.ToString());
                    vals.Add("sord_Discount".ToLower(), saleOrder.discountType.ToString());
                    vals.Add("sord_Disc".ToLower(), saleOrder.amount.ToString());
                    vals.Add("sord_Deposit".ToLower(), "0");


                    StringBuilder n = new StringBuilder();
                    for (int i = 0; i < saleOrder.items.Count; i++)
                    {
                        var item = saleOrder.items[i];

                        if (i == 0)
                            n.Append(i.ToString());
                        else
                            n.Append("," + i.ToString());

                        vals.Add("soit_ItemID".ToLower() + i.ToString(), item.itemID.ToString());
                        vals.Add("soit_Description".ToLower() + i.ToString(), "");
                        vals.Add("soit_Qty".ToLower() + i.ToString(), item.quantity.ToString());
                        vals.Add("soit_Price".ToLower() + i.ToString(), item.price.ToString());

                    }
                    uploadSO(db, vals, n.ToString());
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                HttpContext.Current.Session.Abandon();
            }
            return Ok();
        }


        string uploadSO(sapi.db db, Dictionary<string, string> vals, string mySt)
        {
            string token = Request.Headers.GetValues("token").First();
            UserProfileViewDTO userProfile = Helper.Helper.GetUserProfile(token);

            HttpContext.Current.Session["userid"] = userProfile.id;
            HttpContext.Current.Session["user"] = userProfile.userName;
            string re = "";
            string re2 = "";
            string hid = "";
            sapi.sapi cls = new sapi.sapi();
            string screenItem = "tblSaleOrderItemNew";
            string screen = "tblSaleOrderNew";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            db.beginTran();

            if (!vals.ContainsKey("sord_Date".ToLower()))
            {
                vals.Add("sord_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
            }
            else
            {
                vals.Remove("sord_Date".ToLower());
                vals.Add("sord_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
            }
            if (vals.ContainsKey("invo_daystoexp"))
            {
                double invo_daystoexp = db.cNum(vals["invo_daystoexp"]);
                if (!vals.ContainsKey("sord_Date".ToLower()))
                {
                    vals.Add("sord_EndDate".ToLower(), db.getDate(DateTime.UtcNow.AddDays(invo_daystoexp).AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                }
                else
                {
                    vals.Remove("sord_EndDate".ToLower());
                    vals.Add("sord_EndDate".ToLower(), db.getDate(DateTime.UtcNow.AddDays(invo_daystoexp).AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                }
            }

            if (vals.ContainsKey("sord_saleorderid"))
            {
                if (string.IsNullOrEmpty(vals["sord_saleorderid"]) || vals["sord_saleorderid"] == "0")
                {
                    vals.Remove("sord_saleorderid");
                }
            }

            if (vals.ContainsKey("tbls_tableid"))
            {
                if (!string.IsNullOrEmpty(vals["tbls_tableid"]))
                {
                    db.execData("Update tblTable Set tbls_Status = 'O' Where tbls_TableID = " + vals["tbls_tableid"]);
                    aVal.Add("sord_TableID", vals["tbls_tableid"]);
                }
            }

            string sord_assignedto = "";

            if (vals.ContainsKey("sord_assignedto".ToLower()))
            {
                if (!string.IsNullOrEmpty(vals["sord_assignedto".ToLower()]))
                {
                    sord_assignedto = vals["sord_assignedto".ToLower()];
                }
            }
            if (string.IsNullOrEmpty(sord_assignedto))
            {
                DataTable tmpTbl = db.readData("exec notificationSetup 'SO'");
                if (tmpTbl.Rows.Count > 0)
                    sord_assignedto = tmpTbl.Rows[0][0].ToString();
            }

            if (!vals.ContainsKey("sord_assignedto".ToLower()))
            {
                vals["sord_assignedto"] = sord_assignedto;
            }
            else
            {
                vals.Add("sord_assignedto", sord_assignedto);
            }

            re = cls.saveRecord(screen, vals, db, aVals: aVal, ignoreROF: true);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    hid = (string)str.tbl[0].msg;
                    //foreach (var st in Request.Form["N"].ToString().Split(','))
                    foreach (var st in mySt.ToString().Split(','))
                    {
                        aVal.Clear();
                        aVal.Add("soit_SaleOrderID", hid);
                        if (string.IsNullOrEmpty(st)) continue;
                        Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                        v["soit_total"] = (db.cNum(v["soit_Qty".ToLower()].ToString()) * db.cNum(v["soit_Price".ToLower()].ToString())).ToString();
                        aVal.Add("soit_ShipQty", "0");
                        aVal.Add("soit_RemainQty", v["soit_Qty".ToLower()]);

                        if (vals.ContainsKey("soit_saleorderitemid"))
                        {
                            if (string.IsNullOrEmpty(vals["soit_saleorderitemid"]) || vals["soit_saleorderitemid"] == "0")
                            {
                                vals.Remove("soit_saleorderitemid");
                            }
                        }

                        re2 = cls.saveRecord(screenItem, v, db, aVal, st, ignoreROF: true);
                        str = JsonConvert.DeserializeObject<dynamic>(re2);
                        if (str.tbl != null)
                        {
                            if (str.tbl[0].status != "ok")
                            {
                                db.rollback();
                                return re2;
                            }
                        }
                    }
                    new clsGlobal().SOTotal(hid, db);


                    if (!string.IsNullOrEmpty(sord_assignedto))
                    {
                        db.execData("insert into [dbo].[sys_notification]" +
                            "([notf_Name],[notf_objectID],[notf_objectValue],[notf_Module],[notf_UserID],[notf_FromUserID])" +
                            "values(NULL," +
                            "1035," + hid + ",'SO'," + sord_assignedto + "," + userProfile.id + ")");
                        new clsGlobal().notification(userProfile.id.ToString(), "SO", sord_assignedto, db);
                    }

                }
                else
                {
                    db.rollback();
                    return re;
                }
            }
            db.commit();
            return re;
        }
    }
        
    

    
}
