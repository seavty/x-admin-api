using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Invoice
{
    public class invoiceBase
    {
        public int invo_InvoiceID { get; set; }
        public string invo_Deleted { get; set; }
        public Nullable<int> invo_CreatedBy { get; set; }
        public Nullable<System.DateTime> invo_CreatedDate { get; set; }
        public Nullable<int> invo_UpdatedBy { get; set; }
        public Nullable<System.DateTime> invo_UpdatedDate { get; set; }
        public Nullable<int> invo_ZoneID { get; set; }
        public Nullable<int> invo_WorkflowID { get; set; }
        public Nullable<int> invo_WorkflowItemID { get; set; }
        public Nullable<int> invo_CustomerID { get; set; }
        public Nullable<System.DateTime> invo_Date { get; set; }
        public string invo_Description { get; set; }
        public Nullable<decimal> invo_Disc { get; set; }
        public string invo_Discount { get; set; }
        public Nullable<decimal> invo_DiscountAmount { get; set; }
        public string invo_Name { get; set; }
        public Nullable<int> invo_OpportunityID { get; set; }
        public Nullable<int> invo_QuotationID { get; set; }
        public string invo_Remark { get; set; }
        public Nullable<decimal> invo_SubTotal { get; set; }
        public Nullable<decimal> invo_Total { get; set; }
        public Nullable<decimal> invo_Balance { get; set; }
        public Nullable<decimal> invo_PaidAmount { get; set; }
        public string invo_Status { get; set; }
        public Nullable<int> invo_WarehouseID { get; set; }
        public string invo_isTax { get; set; }
        public Nullable<decimal> invo_Tax { get; set; }
        public Nullable<decimal> invo_GTotal { get; set; }
        public Nullable<int> invo_SaleOrderID { get; set; }
        public Nullable<decimal> invo_CashIn { get; set; }
        public Nullable<decimal> invo_CashIn2 { get; set; }
        public Nullable<decimal> invo_ExRate { get; set; }
        public string invo_isPaid { get; set; }
        public string invo_ViewDetail { get; set; }
        public Nullable<decimal> invo_Deposit { get; set; }
        public string invo_PaymentTerm { get; set; }
        public Nullable<decimal> invo_Cost { get; set; }
        public Nullable<int> invo_MobileSaleID { get; set; }
        public Nullable<int> invo_SalesmanID { get; set; }
        public Nullable<int> invo_PriceListID { get; set; }
        public string invo_Salesman { get; set; }
        public string invo_SalesmanPhone { get; set; }
        public string invo_Company { get; set; }
        public string invo_Province { get; set; }
        public Nullable<decimal> invo_CreditNote { get; set; }
        public string invo_ItemSoldOrder { get; set; }
        public Nullable<System.DateTime> invo_DeliveryDate { get; set; }
        public string invo_isDelivered { get; set; }
        public Nullable<decimal> invo_BadDebt { get; set; }
        public string invo_isBadDebt { get; set; }
        public Nullable<System.DateTime> invo_VoidDate { get; set; }
        public Nullable<int> invo_cancelInvoiceID { get; set; }

        public string cust_Name { get; set; }
    }
}