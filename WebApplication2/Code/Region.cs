using System.Collections.Generic;

namespace WebApplication2.Code
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Territory> Territories { get; set; }
    }
}