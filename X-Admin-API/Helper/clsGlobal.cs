using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Newtonsoft.Json;
//using Microsoft.AspNet.SignalR;
namespace X_Admin_API
{
    public class clsGlobal
    {

        public string notification(string from, string m, string to, sapi.db db)
        {
            string re = "";
            string msg = "";
            string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
            DataTable tbl = db.readData("Select * from vPushNotification Where notf_UserID = " +
                to);
            foreach (DataRow row in tbl.Rows)
            {
                string url = "";
                if (row["notf_Module"].ToString() == "SO")
                {
                    url = baseUrl + "saleorder/saleorder.aspx?sord_saleorderid=" + row["ID"].ToString() +
                        "&notf_notificationid=" + row["notf_notificationid"];
                }
                if (url != "")
                {
                    msg = msg + "<a class='app-bar-element full-size' href='" + url + "' onclick='$(this).remove();'>" +
                            "<span class='mif-checkmark icon fg-green'>" +
                            "</span>" +
                            "<span class='title'>&nbsp;&nbsp;" +
                                row["Name"].ToString() + " - " + row["Date"].ToString() + " , " + row["EndDate"].ToString() +
                             "</span>" +
                         "</a>";
                }
            }

            /*
            if (msg.Length > 0)
            {
                re = msg;
                var context = GlobalHost.ConnectionManager.GetHubContext<SignalR.ChatHub>();
                context.Clients.All.addNewMessageToPage(from,
                    msg,
                    to
                );
            }
            */
            return re;
        }

        public string startProduction(sapi.db db,sapi.sapi cls,string productionid)
        {

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            db.beginTran();
            var re = (db.execData("Update tblProduction Set prdt_Status='Started' Where prdt_ProductionID = " + productionid));
            DataTable tblProductionInp = db.readData("Select * from tblProductionInput inner join tblProduction On prdt_ProductionID = ptip_ProductionID Where ptip_Deleted is null and ptip_ProductionID = " + productionid);
            clsGlobal clsglobal = new clsGlobal();
            foreach (DataRow row in tblProductionInp.Rows)
            {
                string strErr = "";
                double qty = db.cNum(row["ptip_Qty"].ToString());
               
                strErr = clsglobal.stockVerification(db, cls, row["ptip_ItemID"].ToString(), qty,
                    row["prdt_WarehouseID"].ToString(), init_InvoiceID: productionid, module: "production");
                if (strErr.Length > 0)
                {
                    db.rollback();
                    return strErr;
                }
                else
                {
                    
                    re = clsglobal.stockDeduction(db, 
                        row["ptip_ItemID"].ToString(), 
                        row["prdt_WarehouseID"].ToString(),
                        qty);

                    if (re.Length > 0)
                    {
                        tblResult.Rows[0]["status"] = "error";
                        tblResult.Rows[0]["msg"] = "Error Starting Production !";
                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        return re;
                    }

                }

            }
            tblResult.Rows[0]["status"] = "ok";
            tblResult.Rows[0]["msg"] = "";
            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
            db.commit();
            return re;
        }

        public string stockVerification(sapi.db db, sapi.sapi cls, string itemid, double qty, string whid, 
            string init_InvoiceItemID = "", string init_InvoiceID = "",string module = "")
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
        public string stockDeduction(sapi.db db, string itemID, string warehouseID, double quit_Qty, string inid_invoicedetailid = "", string init_InvoiceItemID = "", bool add = false,string module = "")
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

        public void debitNoteTotal(string eid, sapi.db db)
        {
            DataTable tbl = db.readData("Select SUM(isNull(dnit_Total,0)) dnit_Total From tblDebitNoteItem " +
                " Where dnit_Deleted is null and dnit_DebitNoteID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {

                var re = db.execData("Update tblDebitNote Set " +
                    " dbnt_Total = " + db.cNum(row["dnit_Total"].ToString()) +
                    ",dbnt_Balance = " + db.cNum(row["dnit_Total"].ToString()) + " - isNULL(dbnt_UsedAmount,0)" +
                    " Where dbnt_DebitNoteID = " + eid);

            }

        }

        public void creditNoteTotal(string eid, sapi.db db)
        {
            DataTable tbl = db.readData("Select SUM(isNull(cnit_Total,0)) cnit_Total From tblCreditNoteItem " +
                " Where cnit_Deleted is null and cnit_CreditNoteID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {

                var re = db.execData("Update tblCreditNote Set " +
                    " crdn_Total = " + db.cNum(row["cnit_Total"].ToString()) +
                    ",crdn_Balance = " + db.cNum(row["cnit_Total"].ToString()) + " - isNULL(crdn_UsedAmount,0)" +
                    " Where crdn_CreditNoteID = " + eid);

            }

        }

        public void WHBudgetTotal(string eid, sapi.db db, double exAmount = 0)
        {
            DataTable tbl = db.readData("Select SUM(isNull(exps_Total,0)) exps_Total From tblExpense " +
                " Where exps_Deleted is null and exps_WarehouseID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                var re = db.execData("Update tblWarehouse Set " +
                    " ware_BudgetUsed = " + db.cNum(row["exps_Total"].ToString()) +
                    ",ware_BudgetBalance = isNULL(ware_Budget,0) - " + (db.cNum(row["exps_Total"].ToString()) - exAmount) +
                    " Where ware_WarehouseID = " + eid);

            }

        }

        public void cashAdvanceTotal(string eid, sapi.db db)
        {
            DataTable tbl = db.readData("Select SUM(isNull(exps_Total,0)) exps_Total From tblExpense " +
                " Where exps_Deleted is null and exps_CashAdvanceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                var re = db.execData("Update tblCashAdvance Set " +
                    " csad_UsedAmount = " + db.cNum(row["exps_Total"].ToString()) +
                    ",csad_Balance = isNULL(csad_Amount,0) - " + db.cNum(row["exps_Total"].ToString()) +
                    " Where csad_CashAdvanceID = " + eid);

            }

        }

        public void invoiceTotal(string eid, sapi.db db, double deposit = 0, string soid = "")
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

        public void APInvoiceTotal(string eid, sapi.db db)
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;

            DataTable tbl = db.readData("Select * From tblAPInvoice " +
                " Where apiv_Deleted is null and apiv_APInvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["apiv_Discount"].ToString();
                invo_Disc = db.cNum(row["apiv_Disc"].ToString());
                invo_IsTax = db.cNum(row["apiv_IsTax"].ToString());

            }

            tbl = db.readData("Select SUM(isNull(apit_Total,0)) apit_Total From tblAPInvoiceItem " +
                " Where apit_Deleted is null and apit_APInvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["apit_Total"].ToString());
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
                var re = db.execData("Update tblAPInvoice Set " +
                    " apiv_SubTotal = " + invo_SubTotal +
                    ",apiv_DiscountAmount = " + invo_DiscountAmount +
                    ",apiv_Total = " + invo_Total +
                    ",apiv_GTotal = " + invo_GTotal +
                    ",apiv_Tax = " + invo_Tax +
                    ",apiv_Balance = " + invo_GTotal + " - isNULL(apiv_DebitNote,0) - isNull(apiv_Deposit,0) - isNull(apiv_PaidAmount,0) " +
                    " Where apiv_APInvoiceID = " + eid
                    );
            }
        }

        public string receiveItem(sapi.db db, Dictionary<string, string> v, string warehouseID)
        {
            string re = "";
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            var tmp = db.execData("Update tblItemWarehouse Set " +
                " itwh_Qty = isNULL(itwh_Qty,0) + " + db.cNum(v["reit_qty"].ToString()) +
                " where itwh_WarehouseID = " + db.sqlStr(warehouseID) +
                " and itwh_ItemID = " + db.sqlStr(v["reit_itemid"].ToString())
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

                double cost = 0;
                DataTable tblItem = db.readData("Select * from tblItem " +
                                                " Where item_Deleted is null and item_ItemID = " + v["reit_itemid"]);
                foreach (DataRow rowItem in tblItem.Rows)
                {

                    /*cost = (db.cNum(rowItem["reit_Total"].ToString()) + (quit_Qty * quit_Price)) /
                        (db.cNum(rowItem["reit_Qty"].ToString()) + quit_Qty);*/

                    cost = (db.cNum(rowItem["item_Cost"].ToString()) * db.cNum(rowItem["item_Qty"].ToString()) + (quit_Price * quit_Qty)) /
                                                (db.cNum(rowItem["item_Qty"].ToString()) + quit_Qty);
                }
                tmp = db.execData("Declare @tot decimal(18,6) " +
                    " Select @tot = SUM(itwh_Qty) " +
                    " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(v["reit_itemid"].ToString()) +
                    " update tblItem set item_Qty = @tot " +
                    ",item_LastCost = " + quit_Price +
                    ",item_Cost = " + cost +
                    " where item_ItemID = " + db.sqlStr(v["reit_itemid"].ToString())
                );
                if (tmp != "ok")
                {

                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = tmp;
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    return re;
                }

                if (!string.IsNullOrEmpty(v["rece_Date".ToLower()]))
                {
                    var ttt = db.execData("Delete tblItemLog Where itml_ItemID = " +
                        db.sqlStr(v["reit_itemid"].ToString()) +
                        " and cast(itml_Date as date) = " +
                        db.sqlStr(DateTime.Parse(db.getDate(v["rece_Date".ToLower()])).ToString("yyyy-MM-dd"))
                        );

                    ttt = db.execData("Insert Into tblItemLog(itml_ItemID,itml_Cost,itml_Date)" +
                        "VALUES(" + db.sqlStr(v["reit_itemid"].ToString()) +
                        "," + cost + "," + db.sqlStr(db.getDate(v["rece_Date".ToLower()])) +
                        ")");
                }


                DataTable tblItemSet = null;

                if (db.readData("item_isSet", "Select item_isSet From tblItem Where item_Deleted is null and item_ItemID = " + v["reit_itemid"]) == "Y")
                {
                    tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID = " + v["reit_itemid"]);
                }
                if (tblItemSet != null)
                {
                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                    {
                        quit_Qty = 0;
                        quit_Price = 0;
                        if (v.ContainsKey("reit_Qty".ToLower()))
                        {
                            quit_Qty = db.cNum(v["reit_Qty".ToLower()]) * db.cNum(rowItemSet["sitm_Qty"].ToString());
                        }
                        if (v.ContainsKey("reit_Price".ToLower()))
                        {
                            quit_Price = db.cNum(v["reit_Price".ToLower()]);
                        }

                        if (v.ContainsKey("reit_Total".ToLower()))
                        {
                            v["reit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                        }

                        /*string ss = "Update tblItemWarehouse Set " +
                            " itwh_Qty = isNULL(itwh_Qty,0) + " + quit_Qty +
                            " where itwh_WarehouseID = " + db.sqlStr(vals["rece_warehouseid"].ToString()) +
                            " and itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString());*/
                        tmp = db.execData("Update tblItemWarehouse Set " +
                           " itwh_Qty = isNULL(itwh_Qty,0) + " + quit_Qty +
                           " where itwh_WarehouseID = " + db.sqlStr(warehouseID) +
                           " and itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString())
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

                            cost = 0;

                            foreach (DataRow rowItem in tblItem.Rows)
                            {

                                cost = (db.cNum(rowItem["item_Cost"].ToString()) * db.cNum(rowItem["item_Qty"].ToString()) +
                                    (quit_Price * quit_Qty / db.cNum(rowItemSet["sitm_Qty"].ToString()))) /
                                    (db.cNum(rowItem["item_Qty"].ToString()) + (quit_Qty * db.cNum(rowItemSet["sitm_Qty"].ToString())));
                            }

                            tmp = db.execData("Declare @tot decimal(18,6) " +
                                " Select @tot = SUM(itwh_Qty) " +
                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString()) +
                                " update tblItem set item_Qty = @tot " +
                                ",item_LastCost = " + quit_Price +
                                ",item_Cost = " + cost +
                                " where item_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString())
                            );
                            if (tmp != "ok")
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = tmp;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }

                            if (!string.IsNullOrEmpty(v["rece_Date".ToLower()]))
                            {
                                var ttt = db.execData("Delete tblItemLog Where itml_ItemID = " +
                                    db.sqlStr(v["reit_itemid"].ToString()) +
                                    " and cast(itml_Date as date) = " +
                                    db.sqlStr(DateTime.Parse(db.getDate(v["rece_Date".ToLower()])).ToString("yyyy-MM-dd"))
                                    );

                                ttt = db.execData("Insert Into tblItemLog(itml_ItemID,itml_Cost,itml_Date)" +
                                    "VALUES(" + db.sqlStr(v["reit_itemid"].ToString()) +
                                    "," + cost + "," + db.sqlStr(db.getDate(v["rece_Date".ToLower()])) +
                                    ")");
                            }

                        }
                    }
                }

            }
            return re;
        }
        public void ReceiveTotal(string eid, sapi.db db)
        {

            double reit_Total = 0;


            DataTable tbl = db.readData("Select SUM(isNull(reit_Total,0)) reit_Total From tblReceiveItem " +
                " Where reit_Deleted is null and reit_ReceiveID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                reit_Total = db.cNum(row["reit_Total"].ToString());


                db.execData("Update tblReceive Set " +
                    " rece_Total = " + reit_Total +
                    " Where rece_ReceiveID = " + eid
                    );
            }
        }

        public void issueTotal(string eid, sapi.db db)
        {

            double reit_Total = 0;


            DataTable tbl = db.readData("Select SUM(isNull(isit_Total,0)) isit_Total From tblIssueItem " +
                " Where isit_Deleted is null and isit_IssueID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                reit_Total = db.cNum(row["isit_Total"].ToString());


                db.execData("Update tblIssue Set " +
                    " issu_Total = " + reit_Total +
                    " Where issu_IssueID = " + eid
                    );
            }
        }

        public void validInvoice(string id, sapi.db db)
        {
            DataTable tbl = db.readData("select * from tblInvoice" +
                " where invo_Deleted is null and invo_Balance = 0 and invo_InvoiceID = " + id);
            if (tbl.Rows.Count > 0)
            {
                db.execData("Update tblInvoice Set invo_isPaid = 'Y' Where invo_InvoiceID = " + id);
            }
            else
            {
                db.execData("Update tblInvoice Set invo_isPaid = NULL Where invo_InvoiceID = " + id);

            }
        }

        public void validAPInvoice(string id, sapi.db db)
        {
            DataTable tbl = db.readData("select * from tblAPInvoice" +
                " where apiv_Deleted is null and apiv_Balance = 0 and apiv_APInvoiceID = " + id);
            if (tbl.Rows.Count > 0)
            {
                db.execData("Update tblAPInvoice Set apiv_isPaid = 'Y' Where apiv_APInvoiceID = " + id);
            }
            else
            {
                db.execData("Update tblAPInvoice Set apiv_isPaid = NULL Where apiv_APInvoiceID = " + id);

            }
        }

        public void validSO(string id, sapi.db db)
        {
            DataTable tbl = db.readData("select * from tblSaleOrderItem" +
                " where soit_RemainQty>0 and soit_SaleOrderID = " + id);
            if (tbl.Rows.Count <= 0)
            {
                db.execData("Update tblSaleOrder Set sord_isComplete = 'Y' Where sord_SaleOrderID = " + id);
            }
        }


        public void SOTotal(string eid, sapi.db db)
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;

            DataTable tbl = db.readData("Select * From tblSaleOrder " +
                " Where sord_Deleted is null and sord_SaleOrderID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["sord_Discount"].ToString();
                invo_Disc = db.cNum(row["sord_Disc"].ToString());
                invo_IsTax = db.cNum(row["sord_IsTax"].ToString());

            }

            tbl = db.readData("Select SUM(isNull(soit_Total,0)) soit_Total From tblSaleOrderItem " +
                " Where soit_Deleted is null and soit_SaleOrderID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["soit_Total"].ToString());
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

                db.execData("Update tblSaleOrder Set " +
                    " sord_SubTotal = " + invo_SubTotal +
                    ",sord_DiscountAmount = " + invo_DiscountAmount +
                    ",sord_Total = " + invo_Total +
                    ",sord_GTotal = " + invo_GTotal +
                    ",sord_Tax = " + invo_Tax +
                    " Where sord_SaleOrderID = " + eid
                    );
            }
        }

        public void validPO(string id, sapi.db db)
        {
            DataTable tbl = db.readData("select * from tblPurchaseOrderItem" +
                " where poit_RemainQty <=0 " +
                " and isNULL(poit_Qty,0) - isNULL(poit_APInvoiceQty,0) <= 0 " +
                " and poit_PurchaseOrderID = " + id);
            if (tbl.Rows.Count > 0)
            {
                db.execData("Update tblPurchaseOrder Set purc_isComplete = 'Y' Where purc_PurchaseOrderID = " + id);
            }
        }

        public void POTotal(string eid, sapi.db db)
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;

            DataTable tbl = db.readData("Select * From tblPurchaseOrder " +
                " Where purc_Deleted is null and purc_PurchaseOrderID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["purc_Discount"].ToString();
                invo_Disc = db.cNum(row["purc_Disc"].ToString());
                invo_IsTax = db.cNum(row["purc_IsTax"].ToString());

            }

            tbl = db.readData("Select SUM(isNull(poit_Total,0)) poit_Total From tblPurchaseOrderItem " +
                " Where poit_Deleted is null and poit_PurchaseOrderID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["poit_Total"].ToString());
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

                db.execData("Update tblPurchaseOrder Set " +
                    " purc_SubTotal = " + invo_SubTotal +
                    ",purc_DiscountAmount = " + invo_DiscountAmount +
                    ",purc_Total = " + invo_Total +
                    ",purc_GTotal = " + invo_GTotal +
                    ",purc_Tax = " + invo_Tax +
                    " Where purc_PurchaseOrderID = " + eid
                    );
            }
        }

        public string savePreLine(sapi.db db, string screen, Dictionary<string, string> vals, string rowNum)
        {
            sapi.sapi cls = new sapi.sapi();
            //string screen = Request.Form["screen"].ToString();
            string tabl_Name = "";
            DataTable tblScreen = db.readData("Select * from vSys_Screen Where scrn_Name=" +
                db.sqlStr(screen));
            DataTable tblData = null;

            string col = "";
            foreach (DataRow rowScreen in tblScreen.Rows)
            {
                tabl_Name = rowScreen["tabl_Name"].ToString();
                col = col + "[" + rowScreen["cols_Name"].ToString() + "],";
            }
            if (col.Length > 0)
            {
                col = col.Substring(0, col.Length - 1);
                tblData = db.readData("Select " + col + " From " + tabl_Name + " Where 1=2");
                tblData.Rows.Add();
                /*
                Dictionary<string, string> vals = new Dictionary<string, string>();
                foreach (var st in Request.Form.AllKeys)
                {
                    if (!string.IsNullOrEmpty(st))
                    {
                        if (Request.Form[st] != null)
                        {
                            vals.Add(st.ToLower(), Request.Form[st].ToString());
                        }
                    }
                }*/
                foreach (DataColumn c in tblData.Columns)
                {
                    try
                    {
                        if (tblScreen.Select("cols_Name = " + db.sqlStrN(c.ColumnName))[0]["cols_Type"].ToString() == "4")
                        {
                            tblData.Rows[0][c] = db.getDate(vals[c.ToString().ToLower()]);
                        }
                        else
                            if (tblScreen.Select("cols_Name = " + db.sqlStrN(c.ColumnName))[0]["cols_Type"].ToString() == "5")
                            {
                                tblData.Rows[0][c] = db.getDate(vals[c.ToString().ToLower()] + " " + vals[c.ToString().ToLower() + "_hh"] + ":" + vals[c.ToString().ToLower() + "_mm"]);
                            }
                            else
                            {
                                tblData.Rows[0][c] = vals[c.ToString().ToLower()];

                            }
                    }
                    catch (Exception ex) { }
                }
            }
            cls.Mode = global::sapi.sapi.recordMode.Edit;
            return (cls.initCol1(tblScreen, db, tblData.Rows[0],
                rowNum, ""));
        }

        

        public string createCN(string eid, Dictionary<string, string> vals, sapi.db db)
        {
            sapi.sapi cls = new sapi.sapi();
            string re = "";
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            int i = 0;
            db.beginTran();
            DataTable tbl = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_InvoiceID = " +
                eid);
            foreach (DataRow row in tbl.Rows)
            {
                Dictionary<string, string> aVal = new Dictionary<string, string>();
                Dictionary<string, string> nVal = new Dictionary<string, string>();

                //nVal.Add("crdn_Date".ToLower(), db.getDate(DateTime.Parse(row["invo_Date"].ToString()).ToString("yyyy-MM-dd"), 1));
                nVal.Add("crdn_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                nVal.Add("crdn_CustomerID".ToLower(), row["invo_CustomerID"].ToString());
                nVal.Add("crdn_WarehouseID".ToLower(), row["invo_WarehouseID"].ToString());
                nVal.Add("crdn_InvoiceID".ToLower(), row["invo_InvoiceID"].ToString());
                double invo_PaidAmount = db.cNum(row["invo_PaidAmount"].ToString()); // db.cNum(db.readData("invo_PaidAmount", "Select invo_PaidAmount From tblInvoice Where invo_InvoiceID = " + row["invo_InvoiceID"].ToString()));
                re = cls.saveRecord("tblCreditNoteNew", nVal, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        string hid = (string)str.tbl[0].msg;

                        foreach (var st in vals["n"].Split(','))
                        {
                            if (string.IsNullOrEmpty(st))
                                continue;
                            if (db.cNum(vals["init_BQty".ToLower() + st]) > 0)
                            {

                                nVal.Clear();
                                aVal.Clear();
                                aVal.Add("cnit_CreditNoteID", hid);
                                aVal.Add("cnit_InvoiceID".ToLower(), vals["init_invoiceitemid".ToLower() + st]);

                                double quit_Qty = 0;
                                double quit_Price = 0;

                                quit_Qty = db.cNum(vals["init_BQty".ToLower() + st]);
                                double rmqty = 0;
                                double init_Qty = 0;
                                DataTable tblTmp = db.readData("Select * From tblInvoiceItem Where init_Deleted is null and init_InvoiceItemID = " +
                                     vals["init_invoiceitemid".ToLower() + st]);
                                foreach (DataRow rowTmp in tblTmp.Rows)
                                {
                                    rmqty = db.cNum(rowTmp["init_BQty"].ToString());
                                    init_Qty = db.cNum(rowTmp["init_Qty"].ToString());

                                    nVal.Add("cnit_ItemID".ToLower(), rowTmp["init_ItemID"].ToString());
                                    nVal.Add("cnit_Description".ToLower(), rowTmp["init_Description"].ToString());
                                    nVal.Add("cnit_Qty".ToLower(), vals["init_BQty".ToLower() + st]);
                                    nVal.Add("cnit_Price".ToLower(), vals["init_RPrice".ToLower() + st]);

                                }




                                if (quit_Qty > rmqty)
                                {
                                    quit_Qty = rmqty;
                                }
                                if (quit_Qty <= 0)
                                    continue;

                                quit_Price = db.cNum(nVal["cnit_Price".ToLower()]);
                                nVal["cnit_Qty".ToLower()] = quit_Qty.ToString();
                                nVal["cnit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();

                                re = db.execData("Update tblInvoiceItem Set " +
                                    " init_RQty = isNull(init_RQty,0) + " + quit_Qty +
                                    ",init_BQty = isNull(init_Qty,0) - isNull(init_RQty,0) - " + quit_Qty +
                                    " Where init_InvoiceitemID = " + vals["init_invoiceitemid".ToLower() + st]);
                                if (re != "ok")
                                {
                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = re;
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                    return re;
                                }
                                re = cls.saveRecord("tblCreditNoteItemNew", nVal, db, aVals: aVal);
                                str = JsonConvert.DeserializeObject<dynamic>(re);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();

                                        tblResult.Rows[0]["status"] = "error";
                                        tblResult.Rows[0]["msg"] = str.tbl[0].msg;
                                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                        return re;
                                    }
                                    else
                                    {
                                        stockDeduction(db, nVal["cnit_ItemID".ToLower()], row["invo_WarehouseID"].ToString(), quit_Qty);
                                        /*
                                        {
                                            DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                                " Where itwh_ItemID = " + db.sqlStr(vals["init_ItemID".ToLower() + st]) +
                                                " and itwh_WarehouseID = " + db.sqlStr(row["invo_WarehouseID"].ToString()));
                                            foreach (DataRow rowItem in tblItem.Rows)
                                            {
                                                if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                                {
                                                    DataTable tblItemSet = db.readData("Select * from tblInvoiceItemDetail Where inid_Deleted is null and inid_InvoiceItemID=" +
                                                        db.sqlStr(vals["init_InvoiceItemID".ToLower() + st]));
                                                    if (tblItemSet.Rows.Count <= 0)
                                                        tblItemSet.Rows.Add();
                                                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                    {

                                                        double qty = quit_Qty;
                                                        string itemid = vals["init_ItemID".ToLower() + st];
                                                        if (rowItem["item_isSet"].ToString() == "Y")
                                                        {
                                                            //qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString());
                                                            qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString()) / init_Qty;
                                                            itemid = rowItemSet["inid_ItemID"].ToString();
                                                        }
                                                        if (db.readData("item_isStock", "Select item_isStock From tblItem Where item_ItemID = " + itemid) == "Y")
                                                        {
                                                            var tmp = db.execData("Update tblItemWarehouse Set " +
                                                                    " itwh_Qty = isNULL(itwh_Qty,0) + " + qty +
                                                                    " where itwh_WarehouseID = " + db.sqlStr(row["invo_WarehouseID"].ToString()) +
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
                                                                    " Select @tot = SUM(isNull(itwh_Qty,0)) " +
                                                                    " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                                                    " update tblItem set item_Qty = @tot " +
                                                                    " where item_ItemID = " + db.sqlStr(itemid)
                                                                );

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        */
                                        // End of Stock Deduction
                                    }
                                }
                                i++;
                            }
                        }
                        clsGlobal clsglobal = new clsGlobal();
                        clsglobal.creditNoteTotal(hid, db);
                        if (db.cNum(row["invo_Balance"].ToString()) > 0)
                        {
                            double totalCN = db.cNum(db.readData("crdn_Total", "select crdn_Total From tblCreditNote Where crdn_CreditNoteID = " + hid));
                            double cnAmount = totalCN;

                            if (db.cNum(row["invo_Balance"].ToString()) >= totalCN)
                            {

                            }
                            else
                            {
                                cnAmount = db.cNum(row["invo_Balance"].ToString());
                            }
                            var tmp = db.execData("Update tblInvoice Set invo_CreditNote = isNull(invo_CreditNote,0) + " +
                                   cnAmount +
                                    " Where invo_InvoiceID = " + row["invo_InvoiceID"].ToString());
                            clsglobal.invoiceTotal(row["invo_InvoiceID"].ToString(), db);
                            clsglobal.validInvoice(row["invo_InvoiceID"].ToString(), db);
                            tmp = db.execData("Update tblCreditNote Set crdn_UsedAmount = isNULL(crdn_UsedAmount,0) + " + cnAmount +
                                ",crdn_Balance =​ isNULL(crdn_Total,0) - isNULL(crdn_UsedAmount,0) - " + cnAmount +
                                " Where crdn_CreditNoteID = " + hid);
                            tmp = db.execData("Insert into tblInvoiceCN(ivcn_InvoiceID,ivcn_CreditNoteID,ivcn_Amount,ivcn_Date) VALUES(" +
                                    row["invo_InvoiceID"].ToString() + "," + hid + "," + cnAmount + ",GETDATE()" +
                                ")");
                            if (tmp != "ok")
                            {
                                db.rollback();
                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = "No Item To Return !";
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            }
                        }
                    }
                }



            }
            if (i > 0)
            {
                db.commit();
            }
            else
            {
                db.rollback();
                tblResult.Rows[0]["status"] = "error";
                tblResult.Rows[0]["msg"] = "No Item To Return !";
                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

            }


            return re;
        }

        public string invoicePayment(Dictionary<string, string> vals, sapi.db db)
        {
            string re = "";
            sapi.sapi cls = new sapi.sapi();

            Dictionary<string, string> aVal = new Dictionary<string, string>();

            DataTable tbl = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_InvoiceID=" +
                vals["ivpm_invoiceid"]);
            foreach (DataRow row in tbl.Rows)
            {
                aVal.Add("ivpm_WarehouseID", row["invo_WarehouseID"].ToString());
                DataTable tblErr = new DataTable();
                tblErr.Columns.Add("colName");
                tblErr.Columns.Add("msg");
                tblErr.Columns.Add("errType");
                tblErr.Rows.Add();

                if (!vals.ContainsKey("ivpm_date"))
                {
                    vals.Add("ivpm_date", db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                }
                if (!vals.ContainsKey("ivpm_PaymentType".ToLower()))
                {
                    vals.Add("ivpm_PaymentType".ToLower(), "Cash");
                }
                if (db.cNum(vals["ivpm_amount"]) <= 0)
                {
                    tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "ivpm_Amount";
                    tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount must be greater than 0 !";
                    tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                    re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");

                    return re;
                }

                if (db.cNum(db.cNum(vals["ivpm_amount"]).ToString(cls.numFormat)) >
                    db.cNum(db.cNum(row["invo_Balance"].ToString()).ToString(cls.numFormat)))
                {

                    tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "ivpm_Amount";
                    tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount cannot greater than " +
                        (db.cNum(row["invo_Balance"].ToString())).ToString(cls.numFormat) + " !";
                    tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                    re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");

                    return re;
                }

                db.beginTran();
                re = cls.saveRecord("tblInvoicePaymentNew", vals, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {

                        var tmp = db.execData("Update tblInvoice Set " +
                                " invo_PaidAmount = isNull(invo_PaidAmount,0) + " + db.cNum(vals["ivpm_amount"]) +
                                ",invo_Balance = isnull(invo_GTotal,0) - isnull(invo_CreditNote,0) - isnull(invo_Deposit,0) - isnull(invo_PaidAmount,0) - isNULL(invo_BadDebt,0) - " + db.cNum(vals["ivpm_amount"]) +
                                " Where invo_InvoiceID = " + vals["ivpm_invoiceid"]);
                        if (tmp != "ok")
                        {
                            db.rollback();
                            DataTable tblResult = new DataTable();
                            tblResult.Rows.Add();
                            tblResult.Columns.Add("status");
                            tblResult.Columns.Add("msg");
                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = tmp;
                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            db.rollback();
                            return re;
                        }
                        clsGlobal clsglobal = new clsGlobal();
                        clsglobal.validInvoice(vals["ivpm_invoiceid"], db);
                    }
                    else
                    {
                        DataTable tblResult = new DataTable();
                        tblResult.Rows.Add();
                        tblResult.Columns.Add("status");
                        tblResult.Columns.Add("msg");
                        tblResult.Rows[0]["status"] = "error";
                        tblResult.Rows[0]["msg"] = str.tbl[0].msg;
                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        db.rollback();
                        return re;
                    }
                }
            }
            db.commit();
            return re;
        }

        public string createExpense(Dictionary<string, string> vals, sapi.db db)
        {
            string re = "";
            sapi.sapi cls = new sapi.sapi();
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();

            if (vals.ContainsKey("exps_Date".ToLower()))
            {
                vals["exps_Date".ToLower()] = db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1);
            }
            else
            {
                vals.Add("exps_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
            }

            double exTotal = 0;
            if (vals.ContainsKey("exps_expenseid"))
            {
                exTotal = db.cNum(db.readData("exps_Total",
                    "Select exps_Total From tblExpense Where exps_Deleted is null and exps_ExpenseID = " + vals["exps_expenseid"]));
            }
            if (vals.ContainsKey("exps_WarehouseID".ToLower()))
            {
                double bal = db.cNum(db.readData("ware_BudgetBalance", "Select ware_BudgetBalance From tblWarehouse where ware_WarehouseID = " +
                    vals["exps_WarehouseID".ToLower()]));
                if (bal < db.cNum(vals["exps_Total".ToLower()]) - exTotal)
                {
                    DataTable tblResult = new DataTable();
                    tblResult.Rows.Add();
                    tblResult.Columns.Add("colName");
                    tblResult.Columns.Add("msg");
                    tblResult.Columns.Add("errType");
                    tblResult.Columns.Add("status");
                    tblResult.Rows[0]["colName"] = "exps_Total";
                    tblResult.Rows[0]["msg"] = "Not enough balance !";
                    tblResult.Rows[0]["errType"] = "repeat";
                    tblResult.Rows[0]["status"] = "error";

                    return ("{\"error\":" + db.tblToJson(tblResult) + "}");
                }
            }

            re = cls.saveRecord("tblExpenseNew", vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    if (vals.ContainsKey("exps_WarehouseID".ToLower()))
                        new clsGlobal().WHBudgetTotal(vals["exps_WarehouseID".ToLower()], db, exTotal);
                }
                else
                {
                    DataTable tblResult = new DataTable();
                    tblResult.Rows.Add();
                    tblResult.Columns.Add("status");
                    tblResult.Columns.Add("msg");
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = str.tbl[0].msg;
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    db.rollback();
                    return re;
                }
            }
            db.commit();
            return re;
        }

    }
}