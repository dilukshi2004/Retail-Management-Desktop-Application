namespace RetailApp.Models
{
    public class ProductSalesSummary
    {
        public string ProductName { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}