namespace WebApplication2.Code
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual Category Category { get; set; }
        public int QuantityPerUnit { get; set; }
        public decimal PricePerUser { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public bool Discontinued { get; set; }
        public int ReorderLevel { get; set; }
    }
}