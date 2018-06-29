using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using X_Admin_API.Models.DTO.Invoice;

namespace X_Admin_API.Helper
{
    public class xcrm
    {
        public static async Task<string> UploadInvoice(sapi.db db, invoiceDetailListDTO newDTOs, string token)
        {

            Dictionary<string, string> vals = new Dictionary<string, string>();
            var users = Helper.GetUserProfile(token);
            if (users == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Invalid Token !");
            }

            if (newDTOs.results.Count <= 0)
                throw new HttpException((int)HttpStatusCode.NotFound, "No Item(s) !");

            DataTable tblSetting = db.readData("Select * from sys_Setting");

            DataTable tblSalesman = db.readData("Select * from tblSalesman Where salm_SalesmanID = " + db.cNum(users.user_SalesmanID.ToString()));
            if (tblSalesman.Rows.Count <= 0)
            {
                if (tblSetting.Rows[0]["sett_useSalesman"].ToString().ToLower() == "y")
                    throw new HttpException((int)HttpStatusCode.NotFound, "Salesman Not Found !");
            }
            else
            {
                var ttt = tblSetting.Rows[0]["sett_useProvince"].ToString().ToLower();
                if (tblSetting.Rows[0]["sett_useProvince"].ToString().ToLower() == "y")
                    if (String.IsNullOrEmpty(tblSalesman.Rows[0]["salm_Province"].ToString()))
                    {
                        throw new HttpException((int)HttpStatusCode.NotFound, "Salesman's province Not Found !");
                    }
            }

            DataTable tblSett = db.readData("select * from sys_setting");
            if (tblSett.Rows.Count <= 0)
                throw new HttpException((int)HttpStatusCode.NotFound, "Warehouse not Found !");
            string invo_WarehouseID = "";
            foreach (DataRow row in tblSett.Rows)
            {
                invo_WarehouseID = row["sett_WarehouseID"].ToString();
                if (string.IsNullOrEmpty(invo_WarehouseID))
                {
                    throw new HttpException((int)HttpStatusCode.NotFound, "Warehouse not Found !");
                }
            }

            HttpContext.Current.Session["userid"] = users.id;
            HttpContext.Current.Session["user"] = users.userName;
            string re = "";
            string re2 = "";
            string hid = "";
            string errStr = "";
            sapi.sapi cls = new sapi.sapi();
            string screenItem = "tblInvoiceItemNew";
            string screen = "tblInvoiceNew";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            if (db.connect())
            {
                string salm_SalesmanID = "";
                string salm_Province = "";
                if (tblSalesman.Rows.Count > 0)
                {
                    salm_SalesmanID = tblSalesman.Rows[0]["salm_SalesmanID"].ToString();
                    salm_Province = tblSalesman.Rows[0]["salm_Province"].ToString();
                }
                string invo_PriceListID = newDTOs.invo_PriceListID.ToString();
                DataTable tblCust = db.readData("Select * from tblCustomer Where cust_CustomerID = " + db.cNum(newDTOs.invo_CustomerID.ToString()));
                foreach (DataRow row in tblCust.Rows)
                {
                    invo_PriceListID = row["cust_PriceListID"].ToString();
                }


                vals.Add("invo_Date".ToLower(), newDTOs.invo_Date?.ToString("dd/MM/yyyy"));
                vals.Add("invo_Province".ToLower(), salm_Province);
                vals.Add("invo_Company".ToLower(), newDTOs.invo_Company);
                vals.Add("invo_CustomerID".ToLower(), newDTOs.invo_CustomerID.ToString());
                vals.Add("invo_PriceListID".ToLower(), invo_PriceListID);
                vals.Add("invo_WarehouseID".ToLower(), invo_WarehouseID);
                vals.Add("invo_SalesmanID".ToLower(), salm_SalesmanID);
                vals.Add("invo_Status".ToLower(), "New");

                string wh = "";
                string mbid = "";
                DataTable tblMB = db.readData("select * from tblMobileSale " +
                    " inner join tblMobileSaleItem on msit_MobileSaleID = mbsl_MobileSaleID and msit_Deleted is null " +
                    " where mbsl_CheckIn is null and msit_UserID = " + users.id);
                foreach (DataRow rowMB in tblMB.Rows)
                {
                    wh = rowMB["mbsl_warehouseID"].ToString();
                    mbid = rowMB["mbsl_MobileSaleID"].ToString();
                    vals["invo_WarehouseID".ToLower()] = wh;
                    aVal.Add("invo_MobileSaleID", mbid);
                }



                foreach (var item in newDTOs.results)
                {
                    decimal? qty = 0;
                    foreach (var item1 in newDTOs.results)
                    {
                        if (item.init_ItemID == item1.init_ItemID)
                        {
                            qty = qty + item1.init_Qty;
                        }
                    }
                    errStr += stockVerification(db, cls, item.init_ItemID.ToString(), (double)qty, newDTOs.invo_WarehouseID.ToString());
                }
                if (errStr.Length > 0)
                {
                    db.close();
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = errStr;
                    return db.tblToJson(tblResult);
                }

                bool isCredit = false;
                if (vals.ContainsKey("isCredit".ToLower()))
                {
                    isCredit = true;
                }
                db.beginTran();
                if (!vals.ContainsKey("invo_invoiceid"))
                {
                    aVal.Add("invo_WorkflowID", "6");
                    aVal.Add("invo_WorkflowItemID", "12");
                }

                if (vals.ContainsKey("invo_exrate"))
                {
                    aVal.Add("invo_ExRate", vals["invo_exrate".ToLower()]);
                    vals.Remove("invo_exrate".ToLower());
                }
                if (vals.ContainsKey("invo_cashin"))
                {
                    aVal.Add("invo_CashIn", vals["invo_cashin".ToLower()]);
                    vals.Remove("invo_CashIn".ToLower());
                }
                if (vals.ContainsKey("invo_cashin2"))
                {
                    aVal.Add("invo_CashIn2", vals["invo_cashin2".ToLower()]);
                    vals.Remove("invo_cashin2".ToLower());
                }

                if (vals.ContainsKey("invo_status"))
                    vals["invo_status"] = "completed";

                if (vals.ContainsKey("invo_MobileSaleID".ToLower()))
                {
                    aVal.Add("invo_MobileSaleID", vals["invo_MobileSaleID".ToLower()]);
                    vals.Remove("invo_MobileSaleID".ToLower());
                }

                if (isCredit)
                {
                    aVal["invo_CashIn"] = "0";
                    aVal["invo_CashIn2"] = "0";
                }
                if (!vals.ContainsKey("invo_Date".ToLower()))
                {
                    vals.Add("invo_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                }
                else
                {
                    vals.Remove("invo_Date".ToLower());
                    vals.Add("invo_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                }

                re = cls.saveRecord("tblInvoiceNew", vals, db, aVals: aVal, ignoreROF: true);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        hid = (string)str.tbl[0].msg;
                        if (!vals.ContainsKey("invo_invoiceid"))
                        {
                            if (vals.ContainsKey("invo_customerid"))
                            {
                                if (!string.IsNullOrEmpty(vals["invo_customerid"]))
                                {
                                    var tmp = db.execData("Update tblCustomer Set cust_Type='Customer',cust_LastTransDate=GETDATE() Where /*cust_Type='Lead' and*/ cust_CustomerID=" +
                                        vals["invo_customerid"].ToString());
                                    if (tmp != "ok")
                                    {
                                        db.rollback();
                                        throw new HttpException((int)HttpStatusCode.Unauthorized, tmp);
                                    }
                                }
                            }
                        }

                        foreach (var item in newDTOs.results)
                        {
                            Dictionary<string, string> iVals = new Dictionary<string, string>();
                            aVal.Clear();
                            aVal.Add("init_InvoiceID", hid);
                            iVals.Add("init_ItemID".ToLower(), item.init_ItemID.ToString());
                            iVals.Add("init_Description".ToLower(), item.init_Description);
                            iVals.Add("init_Qty".ToLower(), item.init_Qty.ToString());
                            iVals.Add("init_Price".ToLower(), item.init_Price.ToString());
                            iVals.Add("init_Total".ToLower(), (item.init_Price * item.init_Qty).ToString());
                            iVals.Add("init_WarehouseID".ToLower(), newDTOs.invo_WarehouseID.ToString());

                            aVal.Add("init_RPrice", item.init_Price.ToString());
                            aVal.Add("init_BQty", item.init_Price.ToString());

                            re = stockDeduction(db, item.init_ItemID.ToString(), newDTOs.invo_WarehouseID.ToString(), (double)item.init_Qty);
                            if (re == "")
                            {
                                re = cls.saveRecord("tblInvoiceItemNew", iVals, db, aVal, ignoreROF: true);
                                str = JsonConvert.DeserializeObject<dynamic>(re);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        throw new HttpException((int)HttpStatusCode.Unauthorized, "Unable To Save Line Item !");
                                    }
                                }
                            }
                            else
                            {
                                db.rollback();
                                throw new HttpException((int)HttpStatusCode.Unauthorized, "Erro Validate Stock !");
                            }

                        }

                        invoiceTotal(hid, db);

                    }
                }
                db.commit();
                re = hid;
            }
            return re;
        }

        //////////////////
        public static string stockVerification(sapi.db db, sapi.sapi cls, string itemid, double qty, string whid,
            string init_InvoiceItemID = "", string init_InvoiceID = "", string module = "")
        {
            string re = "";
            double exQty = 0;
            if (init_InvoiceItemID != "")
            {

                if (module == "")
                {
                    string hid = db.readData("init_InvoiceID", "Select init_InvoiceID From tblInvoiceItem Where init_InvoiceItemID = " + init_InvoiceItemID);
                    exQty = db.cNum(db.readData("init_Qty",
                        "Select SUM(init_Qty) init_Qty From tblInvoiceItem Where init_Deleted is null and init_ItemID = " +
                        itemid + " and init_InvoiceID=" + hid));
                }
                if (module == "production")
                {
                    string hid = db.readData("ptip_ProductionID", "Select ptip_ProductionID From tblProductionInput Where ptip_ProductionInputID = " + init_InvoiceItemID);
                    exQty = db.cNum(db.readData("ptip_Qty",
                        "Select SUM(ptip_Qty) ptip_Qty From tblProductionInput Where ptip_Deleted is null and ptip_ItemID = " +
                        itemid + " and ptip_ProductionID=" + hid));
                }
            }

            if (init_InvoiceID != "")
            {
                string hid = init_InvoiceID;
                if (module == "")
                {
                    exQty = db.cNum(db.readData("init_Qty",
                        "Select SUM(init_Qty) init_Qty From tblInvoiceItem Where init_Deleted is null and init_ItemID = " +
                        itemid + " and init_InvoiceID=" + hid));
                }
                if (module == "production")
                {
                    exQty = db.cNum(db.readData("ptip_Qty",
                        "Select SUM(ptip_Qty) ptip_Qty From tblProductionInput Where ptip_Deleted is null and ptip_ItemID = " +
                        itemid + " and ptip_ProductionID=" + hid));
                }
            }


            string strErr = "";
            DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                " Where itwh_ItemID = " + db.sqlStr(itemid) +
                " and itwh_WarehouseID = " + db.sqlStr(whid));
            foreach (DataRow rowItem in tblItem.Rows)
            {
                qty = qty - exQty;
                if (rowItem["item_isSet"].ToString() == "Y")
                {
                    DataTable tblItemSet = db.readData("Select * from vSubItem " +
                        " Where sitm_Deleted is null and sitm_ItemID=" + itemid);
                    string prestrErr = "";
                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                    {
                        double initQty = 0;
                        if (rowItemSet["item_IsStock"].ToString() == "Y")
                        {
                            string ttt = "Select itwh_Qty from vItemWarehouse " +
                                " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                " and itwh_WarehouseID = " + db.sqlStr(whid);
                            double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select itwh_Qty from vItemWarehouse " +
                                " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                " and itwh_WarehouseID = " + db.sqlStr(whid)));

                            initQty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                            if (initQty > itemwhQty2)
                            {
                                prestrErr = prestrErr +
                                    "&nbsp;&nbsp;+ " + rowItemSet["item_Name"] + "(" +
                                    initQty + " / " +
                                    itemwhQty2 + ")" +
                                    "<br/>";
                            }
                        }
                    }
                    if (prestrErr.Length > 0)
                    {
                        strErr = strErr + " <h4>- " + rowItem["item_Name"] + " : </h4><br/>" + prestrErr +
                            "<hr class='thin bg-grayLighter'/>";
                    }
                }
                else
                {
                    if (rowItem["item_IsStock"].ToString() == "Y")
                    {
                        if (qty > db.cNum(rowItem["itwh_Qty"].ToString()))
                        {
                            strErr = strErr + " <h4>- " + rowItem["item_Name"] + "(" +
                                qty.ToString() + " / " +
                                db.cNum(rowItem["itwh_Qty"].ToString()) + ")</h3>" + "<hr class='thin bg-grayLighter'/>";
                        }
                    }
                }
            }
            return strErr;
        }
        public static string stockDeduction(sapi.db db, string itemID, string warehouseID, double quit_Qty, string inid_invoicedetailid = "", string init_InvoiceItemID = "", bool add = false, string module = "")
        {
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            string re = "";
            // stock Deduction

            DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                " Where itwh_ItemID = " + db.sqlStr(itemID) +
                " and itwh_WarehouseID = " + db.sqlStr(warehouseID));
            foreach (DataRow rowItem in tblItem.Rows)
            {
                if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                {
                    DataTable tblItemSet = db.readData("Select sitm_Qty inid_Qty,sitm_ItemUsedID inid_ItemID from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                        itemID);

                    if (!string.IsNullOrEmpty(init_InvoiceItemID))
                        tblItemSet = db.readData("Select * from tblInvoiceItemDetail " +
                            " Where inid_Deleted is null and inid_InvoiceItemID=" +
                            init_InvoiceItemID);

                    if (tblItemSet.Rows.Count <= 0)
                    {
                        tblItemSet.Rows.Add();
                    }


                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                    {

                        double qty = quit_Qty;
                        string itemid = itemID;
                        if (rowItem["item_isSet"].ToString() == "Y")
                        {
                            //qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                            //itemid = rowItemSet["sitm_ItemUsedID"].ToString();
                            qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString());
                            itemid = rowItemSet["inid_ItemID"].ToString();
                        }

                        if (rowItem["item_isSet"].ToString() == "Y")
                        {
                            if (inid_invoicedetailid != "")
                            {

                                db.execData("Insert into tblInvoiceItemDetail(inid_InvoiceItemID,inid_ItemID,inid_Qty) VALUES(" +
                                    db.sqlStr(inid_invoicedetailid) + "," + db.sqlStr(itemid) + "," +
                                    qty +
                                    ")");
                            }
                        }

                        string tmp = db.execData("Update tblItemWarehouse Set " +
                            " itwh_Qty = isNULL(itwh_Qty,0) " + (add ? " + " : " - ") + qty +
                                " where itwh_WarehouseID = " + db.sqlStr(warehouseID) +
                                " and itwh_ItemID = " + db.sqlStr(itemid)
                                );
                        if (tmp != "ok")
                        {
                            db.rollback();

                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = tmp;
                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            return re;
                        }
                        else
                        {

                            tmp = db.execData("Declare @tot decimal(18,6) " +
                                " Declare @tot2 decimal(18,6) " +
                                " Select " +
                                " @tot2 = SUM(isNull(itwh_Qty,0)) " +
                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                " update tblItem set  " +
                                " item_Qty = @tot2 " +
                                " where item_ItemID = " + db.sqlStr(itemid)
                            );
                            if (tmp != "ok")
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = tmp;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }
                    }
                }
            }
            // End of stock Deduction
            return re;
        }

        public static void invoiceTotal(string eid, sapi.db db, double deposit = 0, string soid = "")
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;
            double invo_CreditNote = 0;
            double invo_PaidAmount = 0;

            DataTable tbl = db.readData("Select * From tblInvoice " +
                " Where invo_Deleted is null and invo_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                var tmp = row["invo_CreditNote"].ToString();
                invo_Discount = row["invo_Discount"].ToString();
                invo_Disc = db.cNum(row["invo_Disc"].ToString());
                invo_IsTax = db.cNum(row["invo_IsTax"].ToString());
                invo_PaidAmount = db.cNum(row["invo_PaidAmount"].ToString());
                invo_CreditNote = db.cNum(row["invo_CreditNote"].ToString());
            }

            tbl = db.readData("Select SUM(isNull(init_Total,0)) init_Total,SUM(isNull(init_Cost,0) * isNull(init_Qty,0)) totalCost From tblInvoiceItem " +
                " Where init_Deleted is null and init_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["init_Total"].ToString());
                if (invo_Discount == "P")
                {
                    invo_DiscountAmount = (invo_SubTotal * invo_Disc / 100);
                }
                else
                {
                    invo_DiscountAmount = invo_Disc;
                }
                invo_Total = invo_SubTotal - invo_DiscountAmount;

                //Tax Calculation
                invo_Tax = invo_Total * invo_IsTax / 100;
                invo_GTotal = invo_Total + invo_Tax;

                double rDeposit = deposit;
                if (deposit > invo_GTotal)
                {
                    rDeposit = invo_GTotal;
                }
                if (rDeposit > 0 && soid != "")
                {
                    db.execData("Update tblSaleOrder set sord_DepositUsed = IsNULL(sord_DepositUsed,0) + " + rDeposit +
                        " Where sord_SaleOrderID = " + soid);

                }

                var re = db.execData("Update tblInvoice Set " +
                    " invo_SubTotal = " + invo_SubTotal +
                    ",invo_DiscountAmount = " + invo_DiscountAmount +
                    ",invo_Total = " + invo_Total +
                    ",invo_GTotal = " + invo_GTotal +
                    ",invo_Tax = " + invo_Tax +
                    ",invo_CreditNote = " + invo_CreditNote +
                    ",invo_PaidAmount = " + invo_PaidAmount +
                    (soid == "" ? "" : ",invo_Deposit = " + rDeposit) +
                    ",invo_Balance = " + (invo_GTotal - rDeposit - invo_CreditNote) + " - isNull(invo_Deposit,0) - isNull(invo_PaidAmount,0) - isNULL(invo_BadDebt,0) " +
                    ",invo_Cost = " + db.cNum(row["totalCost"].ToString()) +
                    " Where invo_InvoiceID = " + eid
                    );


            }
        }
    }
}