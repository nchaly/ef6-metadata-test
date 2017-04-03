namespace WebApplication2.Code
{
    public class OrderLine
    {
        public int Id { get; set; }
        public virtual Product Product { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }
}