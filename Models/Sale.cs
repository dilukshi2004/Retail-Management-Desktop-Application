using System;
using System.Collections.Generic;

namespace RetailApp.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SaleItem> Items { get; set; } = new();
    }
}