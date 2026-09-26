using LaptopCart.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopCart.Data
{
    public class ApplicationDbContext : DbContext
    {
        /*
         * The ApplicationDbContext class is a subclass of DbContext, which is part of the Entity Framework Core (EF Core) library. 
         * It represents a session with the database and allows you to query and save instances of your entities.
         * In this case, it manages the Product entity.
         */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // The Products property is a DbSet<Product>, which represents the collection of all Product entities in the context.
        public DbSet<Product> Products { get; set; }
    }
}
