using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Uom> Uoms => Set<Uom>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Shape> Shapes => Set<Shape>();

        public DbSet<Specification> Specifications => Set<Specification>();

        public DbSet<ItemSpecification> ItemSpecifications => Set<ItemSpecification>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        }
    }
}