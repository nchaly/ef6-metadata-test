using System.Collections.Generic;

namespace WebApplication2.Code
{
    public class Company
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public virtual Contact Contact { get; set; }
        public virtual Address Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }
}