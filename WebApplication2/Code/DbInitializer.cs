using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Configuration = WebApplication2.Migrations.Configuration;

namespace WebApplication2.Code
{
    public class DbInitializer
    {

        public void InitializeDb(bool noMigration)
        {


            if (noMigration)
            {
                Database.SetInitializer(new NullDatabaseInitializer<Ctx>());
            }
            else
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<Ctx, Configuration>());
            }

            InitAndPopulate();
        }


        void InitAndPopulate()
        {
            using (var c = new Ctx())
            {
                var cnt = c.Categories.Count();
                if (cnt == 0)
                {
                    InitializeData(c, ProgressUpdate);
                    cnt = c.Categories.Count();
                }
                Console.WriteLine($"Database initialized. {cnt} categories found.");
            }
        }

        void ProgressUpdate(int percentDone, string message)
        {
            lock (_sync)
            {
                _percentDone = percentDone;
                _message = message;
            }
        }



        private void InitializeData(Ctx context, Action<int, string> progressUpdate, int seed = 1024)
        {
            progressUpdate(0, "Starting population");
            var rand = new Random(seed);


            // Regions

            var i = 100;
            while (i-- > 0)
            {
                var r = new Region()
                {
                    Name = Faker.Address.UkCounty(),
                    Territories = new List<Territory>()
                };
                var t = rand.Next(2, 5);
                while (t-- > 0)
                {
                    r.Territories.Add(new Territory() { Code = "T" + t, Name = "Territory" + t });
                }

                context.Regions.Add(r);
            }

            context.SaveChanges();
            int percentDone = 0;
            percentDone += 10;
            progressUpdate(percentDone, "Done regions");
            //shippers

            i = 100;
            while (i-- > 0)
            {
                context.Shippers.Add(new Shipper()
                {
                    Name = Faker.Company.Name(),
                    Phone = Faker.Phone.Number()
                });
            }
            context.SaveChanges();
            percentDone += 10;
            progressUpdate(percentDone, "Done shippers"); ;

            //suppliers
            i = 100;

            while (i-- > 0)
            {
                context.Suppliers.Add(new Supplier
                {
                    Address = FakeAddress(context, rand),
                    Name = Faker.Company.Name(),
                    Contact = new Contact() { Name = Faker.Name.FullName(), Title = Faker.Lorem.Words(1).First() },
                    Fax = Faker.Phone.Number(),
                    Phone = Faker.Phone.Number(),
                    HomePage = Faker.Internet.DomainName()
                });
            }
            context.SaveChanges();
            percentDone += 10;
            progressUpdate(percentDone, "Done suppliers");

            //categories

            foreach (var cname in Cnames)
            {
                context.Categories.Add(new Category()
                {
                    Name = cname,
                    Description = cname + " is a cool category"
                });
            }
            context.SaveChanges();
            percentDone += 10;
            progressUpdate(percentDone, "Done categories");

            var suppliers = context.Suppliers.ToArray();
            //products
            var contextCategories = context.Categories.ToArray();
            foreach (var category in contextCategories)
            {
                i = rand.Next(20, 100);
                while (i-- > 0)
                {
                    var p = new Product
                    {
                        Name = "Product" + i + $"({category.Name})",
                        Category = category,
                        Discontinued = Convert.ToBoolean(rand.Next(1)),
                        PricePerUser = (10 ^ rand.Next(3)) * rand.Next(1, 20) / 10.0m,
                        QuantityPerUnit = rand.Next(1, 10),
                        Supplier = suppliers[rand.Next(0, suppliers.Length - 1)],
                        UnitsInStock = rand.Next(4, 2000),
                        UnitsOnOrder = rand.Next(1, 5)
                    };
                    context.Products.Add(p);
                }
                context.SaveChanges();
            }
            percentDone += 10;
            progressUpdate(percentDone, "Done products");
            //companies

            i = 200;
            while (i-- > 0)
            {
                var c = new Company
                {
                    Name = Faker.Company.Name(),
                    Address = FakeAddress(context, rand),
                    Phone = Faker.Phone.Number(),
                    Contact = new Contact() { Name = Faker.Name.FullName(), Title = "Contact" },
                    ExternalId = rand.Next(1000, 5000000).ToString(),
                    Fax = Faker.Phone.Number()
                };
                context.Companies.Add(c);
            }

            context.SaveChanges();
            percentDone += 10;
            progressUpdate(percentDone, "Done products");

            //employees
            var contextCompanies = context.Companies.ToArray();
            foreach (var company in contextCompanies)
            {
                var boss = new Employee()
                {
                    Address = FakeAddress(context, rand),
                    Birthday = new DateTime(rand.Next(1900, 2000), rand.Next(1, 12), rand.Next(1, 27)),
                    Extension = rand.Next(100, 999).ToString(),
                    FirstName = Faker.Name.First(),
                    HiredAt = new DateTime(rand.Next(1900, 2000), rand.Next(1, 12), rand.Next(1, 27)),
                    HomePhone = Faker.Phone.Number(),
                    LastName = Faker.Name.Last(),
                    Company = company
                };
                context.Employees.Add(boss);
                context.SaveChanges();
                i = rand.Next(5, 20);
                while (i-- > 0)
                {
                    var e = new Employee()
                    {
                        Address = FakeAddress(context, rand),
                        Birthday = new DateTime(rand.Next(1900, 2000), rand.Next(1, 12), rand.Next(1, 27)),
                        Extension = rand.Next(100, 999).ToString(),
                        FirstName = Faker.Name.First(),
                        HiredAt = new DateTime(rand.Next(1900, 2000), rand.Next(1, 12), rand.Next(1, 27)),
                        HomePhone = Faker.Phone.Number(),
                        LastName = Faker.Name.Last(),
                        ReportsTo = boss,
                        Company = company
                    };
                    context.Employees.Add(e);
                }
                context.SaveChanges();
            }
            percentDone += 10;
            progressUpdate(percentDone, "Done products");

            //orders

            var companies = context.Companies.ToArray();
            var shippers = context.Shippers.ToArray();
            var products = context.Products.ToArray();

            i = 1000;
            while (i-- > 0)
            {
                var c = companies[rand.Next(0, companies.Length - 1)];
                var e = c.Employees.ToArray();
                var employee = e[rand.Next(0, e.Length - 1)];
                var order = new Order
                {
                    Company = c,
                    Employee = employee,
                    Lines = new List<OrderLine>(),
                    OrderedAt = DateTime.Now - new TimeSpan(10000L * rand.Next(1, 30) * 3600 * 24 + 10000L * rand.Next(1, 10) * 3600),
                    RequireAt = DateTime.Now + new TimeSpan(10000L * rand.Next(1, 10) * 3600 * 24 + 10000L * rand.Next(1, 10) * 3600),
                    ShipTo = employee.Address,
                    ShipVia = shippers[rand.Next(0, shippers.Length - 1)],
                    ShippedAt = null,
                };

                var l = rand.Next(1, 10);
                while (l-- > 0)
                {
                    var p = products[rand.Next(products.Length - 1)];
                    var line = new OrderLine()
                    {
                        Discount = 0,
                        PricePerUnit = p.PricePerUser,
                        Product = p,
                    };
                    order.Lines.Add(line);
                }
                context.Orders.Add(order);

                if ((i % 50) == 0)
                {
                    context.SaveChanges();
                }
            }
            context.SaveChanges();

            percentDone = 100;
            progressUpdate(percentDone, "Population complete");
        }

        static Address FakeAddress(Ctx ctx, Random r)
        {
            var rg = ctx.Regions.ToList();
            return new Address()
            {
                City = Faker.Address.City(),
                Country = Faker.Address.Country(),
                Line1 = Faker.Address.StreetAddress(),
                PostalCode = Faker.Address.ZipCode(),
                Region = rg[r.Next(0, rg.Count - 1)]
            };
        }

        private static readonly string[] Cnames = new[]
        {
            "Automotive & Powersports",
            "Baby Products (Excluding Apparel)",
            "Beauty",
            "Books",
            "Business Products (B2B)",
            "Camera & Photo",
            "Cell Phones",
            "Clothing & Accessories",
            "Collectible Coins",
            "Electronics (Accessories)",
            "Electronics (Consumer)",
            "Fine Art",
            "Grocery & Gourmet Food",
            "Handmade",
            "Health & Personal Care",
            "Historical & Advertising Collectibles",
            "Home & Garden",
            "Industrial & Scientific",
            "Jewelry",
            "Luggage & Travel Accessories",
            "Music",
            "Musical Instruments",
            "Office Products",
            "Outdoors",
            "Personal Computers",
            "Professional Services",
            "Shoes, Handbags & Sunglasses",
            "Software & Computer Games",
            "Sports",
            "Sports Collectibles",
            "Tools & Home Improvement",
            "Toys & Games",
            "Video, DVD & Blu-Ray",
            "Video Games & Video Game Consoles",
            "Watches",
            "Wine"

        };

        private string _message;
        private int _percentDone;
        private object _sync = new object();

        public void GetProgress(out int percent, out string msg)
        {
            lock (_sync)
            {
                percent = _percentDone;
                msg = _message;
            }
        }
    }
}