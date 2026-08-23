using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Application.Services;
using AjayIndustriesERP.Infrastructure.Pdf;
using AjayIndustriesERP.Infrastructure.Persistence;
using AjayIndustriesERP.Infrastructure.Repositories;


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AjayIndustriesERP.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            string webRootPath)
        {
            // =================================================
            // DATABASE
            // =================================================

            services.AddDbContext<ApplicationDbContext>(
                options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString(
                            "DefaultConnection")));


            // =================================================
            // COMPANY
            // =================================================

            services.AddScoped<
                ICompanyRepository,
                CompanyRepository>();

            services.AddScoped<
                ICompanyService,
                CompanyService>();


            // =================================================
            // EMPLOYEE
            // =================================================

            services.AddScoped<
                IEmployeeRepository,
                EmployeeRepository>();

            services.AddScoped<
                IEmployeeService,
                EmployeeService>();


            // =================================================
            // UOM
            // =================================================

            services.AddScoped<
                IUomRepository,
                UomRepository>();

            services.AddScoped<
                IUomService,
                UomService>();


            // =================================================
            // WAREHOUSE
            // =================================================

            services.AddScoped<
                IWarehouseRepository,
                WarehouseRepository>();

            services.AddScoped<
                IWarehouseService,
                WarehouseService>();


            // =================================================
            // ITEM CATEGORY
            // =================================================

            services.AddScoped<
                IItemCategoryRepository,
                ItemCategoryRepository>();

            services.AddScoped<
                IItemCategoryService,
                ItemCategoryService>();


            // =================================================
            // BRAND
            // =================================================

            services.AddScoped<
                IBrandRepository,
                BrandRepository>();

            services.AddScoped<
                IBrandService,
                BrandService>();


            // =================================================
            // ITEM
            // =================================================

            services.AddScoped<
                IItemRepository,
                ItemRepository>();

            services.AddScoped<
                IItemService,
                ItemService>();


            // =================================================
            // SHAPE
            // =================================================

            services.AddScoped<
                IShapeRepository,
                ShapeRepository>();

            services.AddScoped<
                IShapeService,
                ShapeService>();


            // =================================================
            // SPECIFICATION
            // =================================================

            services.AddScoped<
                ISpecificationRepository,
                SpecificationRepository>();

            services.AddScoped<
                ISpecificationService,
                SpecificationService>();


            // =================================================
            // ITEM SPECIFICATION
            // =================================================

            services.AddScoped<
                IItemSpecificationRepository,
                ItemSpecificationRepository>();


            // =================================================
            // SUPPLIER
            // =================================================

            services.AddScoped<
                ISupplierRepository,
                SupplierRepository>();

            services.AddScoped<
                ISupplierService,
                SupplierService>();


            // =================================================
            // DRAWING
            // =================================================

            services.AddScoped<
                IDrawingRepository,
                DrawingRepository>();

            services.AddScoped<
                IDrawingService,
                DrawingService>();

            // Customer Drawing
            services.AddScoped<
                ICustomerDrawingRepository,
                CustomerDrawingRepository>();

            services.AddScoped<
                ICustomerDrawingService,
                CustomerDrawingService>();

            // =================================================
            // PURCHASE ORDER
            // =================================================

            services.AddScoped<
                IPurchaseOrderRepository,
                PurchaseOrderRepository>();

            services.AddScoped<
                IPurchaseOrderService,
                PurchaseOrderService>();


            // =================================================
            // PURCHASE ORDER PDF
            // =================================================

            services.AddScoped<
                IPurchaseOrderPdfService>(
                    _ =>
                        new PurchaseOrderPdfService(
                            webRootPath));

            services.AddScoped<IGoodsReceiptNoteRepository, GoodsReceiptNoteRepository>();
            services.AddScoped<IGoodsReceiptNoteService, GoodsReceiptNoteService>();

            #region Customer Master

            services.AddScoped<
                ICustomerRepository,
                CustomerRepository>();

            services.AddScoped<
                ICustomerService,
                CustomerService>();

            #endregion

            #region Customer Purchase Order

            services.AddScoped<
                ICustomerPurchaseOrderRepository,
                CustomerPurchaseOrderRepository>();

            services.AddScoped<
                ICustomerPurchaseOrderService,
                CustomerPurchaseOrderService>();

            #endregion

            #region Machine Master

            services.AddScoped<
                IMachineRepository,
                MachineRepository>();

            services.AddScoped<
                IMachineService,
                MachineService>();

            #endregion

            #region Production Operation Master

            services.AddScoped<
                IProductionOperationRepository,
                ProductionOperationRepository>();

            services.AddScoped<
                IProductionOperationService,
                ProductionOperationService>();

            #endregion

            #region Item Process Routing

            services.AddScoped<
                IItemProcessRoutingRepository,
                ItemProcessRoutingRepository>();

            services.AddScoped<
                IItemProcessRoutingService,
                ItemProcessRoutingService>();

            #endregion

            #region Production Job

            services.AddScoped<
                IProductionJobRepository,
                ProductionJobRepository>();

            services.AddScoped<
                IProductionJobService,
                ProductionJobService>();

            #endregion

            #region Pre-Dispatch Inspection

            services.AddScoped<
                IPreDispatchInspectionRepository,
                PreDispatchInspectionRepository>();

            services.AddScoped<
                IPreDispatchInspectionService,
                PreDispatchInspectionService>();

            services.AddScoped<
    IPreDispatchInspectionPdfGenerator,
    PreDispatchInspectionPdfGenerator>();

            #endregion

            return services;
        }
    }
}