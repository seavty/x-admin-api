using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using X_Admin_API.Models.DTO.ReceiveItem;
using X_Admin_API.Models.DTO.User;

namespace X_Admin_API.Controllers
{
    public class ReceiveController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "receives";

        [HttpPost]
        [Route(route)]
        public IHttpActionResult ReceiveItem(ReceiveItemNewDTO receiveItem)
        {
            string token = Request.Headers.GetValues("token").First();
            UserProfileViewDTO userProfile = Helper.Helper.GetUserProfile(token);

            HttpContext.Current.Session["userid"] = userProfile.id;
            HttpContext.Current.Session["user"] = userProfile.userName;
            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    vals.Add("rece_WarehouseID".ToLower(), receiveItem.warehouseID.ToString());
                    vals.Add("rece_Date".ToLower(), DateTime.Now.ToString("dd/MM/yyyy"));
                    vals.Add("rece_ReceivedBy".ToLower(), userProfile.id.ToString());
                    vals.Add("rece_Remark".ToLower(), receiveItem.remarks);


                    StringBuilder n = new StringBuilder();
                    for (int i=0; i<receiveItem.items.Count; i++)
                    {
                        var item = receiveItem.items[i];
                        
                        if (i ==0)
                            n.Append(i.ToString());
                        else
                            n.Append("," + i.ToString());

                        vals.Add("reit_ItemID".ToLower() + i.ToString(), item.id.ToString());
                        vals.Add("reit_Qty".ToLower() + i.ToString(), item.quantity.ToString());
                        vals.Add("reit_Price".ToLower() + i.ToString(), item.cost.ToString());

                    }
                    ReceiveItemFunc(db, vals, n.ToString());
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

        //-- function -- 
        string ReceiveItemFunc(sapi.db db, Dictionary<string, string> vals, string mySt)
        {
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            string re = "";
            string screen = "tblReceiveNew";
            string screenItem = "tblReceiveItemNew";
            string whid = "0";
            sapi.sapi cls = new sapi.sapi();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVal, ignoreROF: true);
            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    whid = vals["rece_WarehouseID".ToLower()];
                    clsGlobal clsglobal = new clsGlobal();
                    string hid = (string)str.tbl[0].msg;
                    aVal.Clear();
                    aVal.Add("reit_receiveid", hid);
                    //foreach (var st in Request.Form["N"].ToString().Split(','))
                    foreach (var st in mySt.ToString().Split(','))
                    {
                        {
                            string re2 = "";
                            Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                            v.Add("rece_Date".ToLower(), vals["rece_Date".ToLower()]);
                            double quit_Qty = 0;
                            double quit_Price = 0;
                            if (v.ContainsKey("reit_Qty".ToLower()))
                            {
                                quit_Qty = db.cNum(v["reit_Qty".ToLower()]);
                            }
                            if (v.ContainsKey("reit_Price".ToLower()))
                            {
                                quit_Price = db.cNum(v["reit_Price".ToLower()]);
                            }

                            if (v.ContainsKey("reit_Total".ToLower()))
                            {
                                v["reit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                            }
                            else
                            {
                                v.Add("reit_Total".ToLower(), (quit_Qty * quit_Price).ToString());
                            }
                            re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                            str = JsonConvert.DeserializeObject<dynamic>(re2);
                            if (str.tbl != null)
                            {
                                if (str.tbl[0].status != "ok")
                                {
                                    db.rollback();
                                    return re2;
                                }
                                else
                                {
                                    clsglobal.receiveItem(db, v, whid);
                                }
                            }
                        }
                    }
                    clsglobal.ReceiveTotal(hid, db);
                }
            }
            db.commit();
            return re;
        }
    }
}
