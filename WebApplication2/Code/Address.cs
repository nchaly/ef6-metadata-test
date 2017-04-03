namespace WebApplication2.Code
{
    public class Address
    {
        public int Id { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public virtual Region Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}