using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using( var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        public static void SeedData(AppDbContext context, bool isProd)
        {

            if (isProd)
            {
                Console.WriteLine("Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not run migrations: {ex.Message}");
                }
                
            }


            if (!context.Platforms.Any())
            {
                Console.WriteLine("Seeding data...");

                context.Platforms.AddRange(
                    new Platform() { Name = ".NET Core", Publisher = "Microsoft", Cost ="Free"},
                    new Platform() {  Name = " SQL Server", Publisher = "Microsoft", Cost = "Free"},
                    new Platform() {  Name = "Java", Publisher="Oracle", Cost="Free"}
                );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("We Already have data!");
            }
        }
    }
}
