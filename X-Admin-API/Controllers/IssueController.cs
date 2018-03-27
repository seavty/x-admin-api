using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using X_Admin_API.Models.DTO.IssueItem;
using X_Admin_API.Models.DTO.User;

namespace X_Admin_API.Controllers
{
    public class IssueController : ApiController
    {
        private const string route = Helper.Helper.apiVersion + "issues";

        [HttpPost]
        [Route(route)]
        public IHttpActionResult IssueItem(IsseItemNewDTO issueItem)
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
                    vals.Add("issu_WarehouseID".ToLower(), issueItem.warehouseID.ToString());
                    vals.Add("issu_Date".ToLower(), DateTime.Now.ToString("dd/MM/yyyy"));
                    vals.Add("issu_IssueBy".ToLower(), userProfile.id.ToString());
                    vals.Add("issu_Remark".ToLower(), issueItem.remarks);

                    StringBuilder n = new StringBuilder();
                    for (int i = 0; i < issueItem.items.Count; i++)
                    {
                        var item = issueItem.items[i];
                        if (i == 0)
                            n.Append(i.ToString());
                        else
                            n.Append("," + i.ToString());

                        vals.Add("isit_ItemID".ToLower() + i.ToString(), item.id.ToString());
                        vals.Add("isit_Qty".ToLower() + i.ToString(), item.quantity.ToString());
                        vals.Add("isit_Price".ToLower() + i.ToString(), item.price.ToString());

                    }
                    
                    IssueItemFunc(db, vals, n.ToString());
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


        string IssueItemFunc(sapi.db db, Dictionary<string, string> vals, string mySt)
        {
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            string re = "";
            string screen = "tblIssueNew";
            string screenItem = "tblIssueItemNew";
            string whid = "0";
            sapi.sapi cls = new sapi.sapi();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVal, ignoreROF: true);
            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    whid = vals["issu_WarehouseID".ToLower()];
                    clsGlobal clsglobal = new clsGlobal();
                    string hid = (string)str.tbl[0].msg;
                    aVal.Clear();
                    aVal.Add("isit_issueid", hid);
                    //foreach (var st in Request.Form["N"].ToString().Split(','))
                    foreach (var st in mySt.ToString().Split(','))
                    {

                        {
                            string re2 = "";
                            Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);


                            double quit_Qty = 0;
                            double quit_Price = 0;
                            if (v.ContainsKey("isit_Qty".ToLower()))
                            {
                                quit_Qty = db.cNum(v["isit_Qty".ToLower()]);
                            }
                            if (v.ContainsKey("isit_Price".ToLower()))
                            {
                                quit_Price = db.cNum(v["isit_Price".ToLower()]);
                            }

                            if (v.ContainsKey("isit_Total".ToLower()))
                            {
                                v["isit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                            }
                            else
                            {
                                v.Add("isit_Total".ToLower(), (quit_Qty * quit_Price).ToString());
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
                                    clsglobal.stockDeduction(db, v["isit_itemid"], whid, db.cNum(v["isit_qty"]));
                                }
                            }
                        }
                    }
                    clsglobal.issueTotal(hid, db);
                }
            }
            db.commit();
            return re;
        }
    }
}
