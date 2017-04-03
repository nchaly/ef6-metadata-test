using System;
using System.Collections.Generic;

namespace WebApplication2.Code
{
    public class Order
    {
        public int Id { get; set; }
        public virtual Company Company { get; set; }
        public virtual Employee Employee { get; set; }
        public DateTime OrderedAt { get; set; }
        public DateTime RequireAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public virtual Address ShipTo { get; set; }
        public virtual Shipper ShipVia { get; set; }
        public decimal Freight { get; set; }
        public virtual ICollection<OrderLine> Lines { get; set; }
    }
}