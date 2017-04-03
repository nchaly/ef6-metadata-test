namespace WebApplication2.Migrations
{
    using System.Data.Entity.Migrations;

    public sealed class Configuration : DbMigrationsConfiguration<WebApplication2.Code.Ctx>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WebApplication2.Code.Ctx context)
        {
        }
    }
}
