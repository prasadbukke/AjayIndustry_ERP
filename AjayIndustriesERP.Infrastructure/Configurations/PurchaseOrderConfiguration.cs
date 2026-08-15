using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PurchaseOrderConfiguration
        : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("PurchaseOrders");

            builder.HasKey(x => x.Id);


            // Code = PO Number
            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Code)
                .IsUnique();


            // PO Information
            builder.Property(x => x.PODate)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();


            // Supplier Snapshot
            builder.Property(x => x.SupplierName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.SupplierAddress)
                .HasMaxLength(1000);

            builder.Property(x => x.SupplierGSTIN)
                .HasMaxLength(20);

            builder.Property(x => x.SupplierContactPerson)
                .HasMaxLength(150);

            builder.Property(x => x.SupplierPhone)
                .HasMaxLength(30);

            builder.Property(x => x.SupplierEmail)
                .HasMaxLength(200);


            // Delivery / Terms
            builder.Property(x => x.DeliveryAddress)
                .HasMaxLength(1000);

            builder.Property(x => x.PaymentTerms)
                .HasMaxLength(500);

            builder.Property(x => x.DeliveryTerms)
                .HasMaxLength(500);

            builder.Property(x => x.TermsAndConditions)
                  .HasMaxLength(4000);

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            builder.Property(x => x.CancellationReason)
                .HasMaxLength(500);


            // Amounts
            builder.Property(x => x.SubTotal)
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TaxableAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.CGSTAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.SGSTAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.IGSTAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TransportCharges)
                .HasPrecision(18, 2);

            builder.Property(x => x.OtherCharges)
                .HasPrecision(18, 2);

            builder.Property(x => x.RoundOffAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.GrandTotal)
                .HasPrecision(18, 2);


            // BaseEntity
            builder.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);


            // Supplier Relation
            builder.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);


            // Purchase Order Items
            builder.HasMany(x => x.Items)
                .WithOne(x => x.PurchaseOrder)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company Snapshot
            builder.Property(x => x.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.CompanyAddress)
                .HasMaxLength(1000);

            builder.Property(x => x.CompanyState)
                .HasMaxLength(100);

            builder.Property(x => x.CompanyGSTIN)
                .HasMaxLength(20);

            builder.Property(x => x.CompanyPhone)
                .HasMaxLength(30);

            builder.Property(x => x.CompanyEmail)
                .HasMaxLength(200);


            // Company Relation
            builder.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}