//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace X_Admin_API.Models.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblSaleOrder
    {
        public int id { get; set; }
        public string sord_Deleted { get; set; }
        public Nullable<int> sord_CreatedBy { get; set; }
        public Nullable<System.DateTime> sord_CreatedDate { get; set; }
        public Nullable<int> sord_UpdatedBy { get; set; }
        public Nullable<System.DateTime> sord_UpdatedDate { get; set; }
        public Nullable<int> sord_WorkflowID { get; set; }
        public Nullable<int> sord_WorkflowItemID { get; set; }
        public Nullable<int> sord_ZoneID { get; set; }
        public string saleOrderNo { get; set; }
        public Nullable<System.DateTime> sord_Date { get; set; }
        public Nullable<System.DateTime> sord_EndDate { get; set; }
        public Nullable<decimal> sord_Disc { get; set; }
        public string sord_Discount { get; set; }
        public Nullable<decimal> sord_DiscountAmount { get; set; }
        public Nullable<decimal> sord_GTotal { get; set; }
        public string sord_isTax { get; set; }
        public string sord_Remark { get; set; }
        public Nullable<decimal> sord_SubTotal { get; set; }
        public Nullable<decimal> sord_Tax { get; set; }
        public Nullable<decimal> total { get; set; }
        public Nullable<int> warehouseID { get; set; }
        public Nullable<int> customerID { get; set; }
        public string sord_isComplete { get; set; }
        public Nullable<decimal> sord_Deposit { get; set; }
        public Nullable<decimal> sord_DepositUsed { get; set; }
        public Nullable<int> sord_PriceListID { get; set; }
        public string sord_Province { get; set; }
        public string sord_Company { get; set; }
        public Nullable<int> sord_SalesmanID { get; set; }
    }
}