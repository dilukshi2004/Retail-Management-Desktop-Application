namespace RetailApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = "pcs";
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
    }
}