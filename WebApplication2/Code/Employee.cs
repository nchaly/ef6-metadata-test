using System;
using System.Collections.Generic;

namespace WebApplication2.Code
{
    public class Employee
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public virtual Address Address { get; set; }
        public DateTime HiredAt { get; set; }
        public DateTime Birthday { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public virtual Employee ReportsTo { get; set; }
        public string Notes { get; set; }

        public virtual ICollection<string> Territories { get; set; }
        public virtual Company Company { get; set; }
    }
}