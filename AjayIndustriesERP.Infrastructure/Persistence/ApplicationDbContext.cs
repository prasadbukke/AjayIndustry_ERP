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
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Drawing> Drawings =>  Set<Drawing>();
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
        public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; }
        public DbSet<Customer> Customers { get; set; }

        #region Customer Purchase Order

        public DbSet<CustomerPurchaseOrder>
            CustomerPurchaseOrders
        { get; set; }

        public DbSet<CustomerPurchaseOrderItem>
            CustomerPurchaseOrderItems
        { get; set; }

        #endregion

        #region Machine Master

        public DbSet<Machine> Machines { get; set; }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        }
    }
}