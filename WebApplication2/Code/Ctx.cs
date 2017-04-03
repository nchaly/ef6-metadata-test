using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Configuration = WebApplication2.Migrations.Configuration;

namespace WebApplication2.Code
{
    public class Ctx : DbContext
    {
        public Ctx() : base("DefaultConnection")
        {
        }

        public static readonly DbInitializer Initializer = new DbInitializer(); 
        

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Address> Addresss { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Territory> Territories { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

    }
}