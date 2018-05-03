using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Invoice
{
    public class invoiceItemBase
    {
        public int init_InvoiceItemID { get; set; }
        public string init_Deleted { get; set; }
        public Nullable<int> init_CreatedBy { get; set; }
        public Nullable<System.DateTime> init_CreatedDate { get; set; }
        public Nullable<int> init_UpdatedBy { get; set; }
        public Nullable<System.DateTime> init_UpdatedDate { get; set; }
        public Nullable<int> init_ZoneID { get; set; }
        public Nullable<int> init_WorkflowID { get; set; }
        public Nullable<int> init_WorkflowItemID { get; set; }
        public string init_Description { get; set; }
        public Nullable<decimal> init_Price { get; set; }
        public Nullable<decimal> init_Qty { get; set; }
        public Nullable<int> init_InvoiceID { get; set; }
        public Nullable<decimal> init_Total { get; set; }
        public Nullable<int> init_ItemID { get; set; }
        public Nullable<decimal> init_RPrice { get; set; }
        public Nullable<decimal> init_RQty { get; set; }
        public Nullable<decimal> init_BQty { get; set; }
        public Nullable<decimal> init_Cost { get; set; }
        public Nullable<decimal> init_LastCost { get; set; }
        public Nullable<int> init_WarehouseID { get; set; }
        public string init_Unit { get; set; }
        public string init_Disc { get; set; }
        public Nullable<decimal> init_DiscAmount { get; set; }
        public Nullable<int> init_SaleOrderItemID { get; set; }
    }
}