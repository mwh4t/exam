namespace korzunov.models
{
    public class Product
    {
        public string Article { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public string ManufacturerName { get; set; }
    }
}