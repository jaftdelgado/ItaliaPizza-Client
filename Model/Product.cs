namespace ItaliaPizzaClient.Model
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public bool IsPrepared { get; set; }
        public int SupplierID { get; set; }
        public string Status { get; set; }
        public byte[] Photo { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public Supplier supplier { get; set; }

    }
}
