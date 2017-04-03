namespace WebApplication2.Code
{
    public class Supplier
    {
        public int Id { get; set; }
        public virtual Contact Contact { get; set; }
        public string Name { get; set; }
        public virtual Address Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string HomePage { get; set; }
    }
}