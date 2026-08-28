/*
============================================================
File: InvoiceItemConfiguration.cs

Module:
Invoice

Purpose:
Configures InvoiceItem entity for Entity Framework Core.

Responsibilities:
- Configure Invoice Item table.
- Configure source snapshot field lengths.
- Configure financial decimal precision.
- Configure optional Delivery Challan relationships.
- Configure Production Job based Invoice source uniqueness.
- Configure indexes required by Invoice workflow.

Important:
- New Invoice flow is based on Completed Production Jobs.
- Delivery Challan is optional.
- One Production Job cannot appear twice in the same Invoice.
- Partial invoicing across multiple Invoices remains allowed.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class InvoiceItemConfiguration
        : IEntityTypeConfiguration<InvoiceItem>
    {
        #region Configure

        public void Configure(
            EntityTypeBuilder<InvoiceItem> builder)
        {
            #region Table And Primary Key

            builder.ToTable(
                "InvoiceItems");


            builder.HasKey(
                x => x.Id);

            #endregion


            #region Identification

            builder.Property(
                    x => x.SequenceNumber)
                .IsRequired();

            #endregion


            #region Delivery Challan Source - Optional

            builder.Property(
                    x => x.DeliveryChallanCode)
                .HasMaxLength(
                    50);


            builder.Property(
                    x => x.DeliveryChallanQuantity)
                .HasPrecision(
                    18,
                    3);


            /*
             * Delivery Challan is optional.
             *
             * New Invoice can be created directly from
             * Completed Production Job even if Challan
             * has not yet been created.
             */
            builder.HasOne(
                    x => x.DeliveryChallan)
                .WithMany()
                .HasForeignKey(
                    x => x.DeliveryChallanId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasOne(
                    x => x.DeliveryChallanItem)
                .WithMany()
                .HasForeignKey(
                    x => x.DeliveryChallanItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Product / Item Snapshot

            builder.Property(
                    x => x.ProductReference)
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.ItemName)
                .IsRequired()
                .HasMaxLength(
                    250);


            builder.Property(
                    x => x.PartNumber)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.CustomerItemCode)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.UnitName)
                .HasMaxLength(
                    50);


            builder.Property(
                    x => x.HsnNumber)
                .HasMaxLength(
                    50);

            #endregion


            #region Customer Purchase Order Snapshot

            builder.Property(
                    x => x.CustomerPurchaseOrderCode)
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.CustomerPurchaseOrderNumber)
                .HasMaxLength(
                    150);

            #endregion


            #region Production Job Snapshot

            builder.Property(
                    x => x.ProductionJobCode)
                .HasMaxLength(
                    100);

            #endregion


            #region Quantity

            builder.Property(
                    x => x.InvoiceQuantity)
                .HasPrecision(
                    18,
                    3)
                .IsRequired();

            #endregion


            #region Commercial Values

            builder.Property(
                    x => x.Rate)
                .HasPrecision(
                    18,
                    4)
                .IsRequired();


            builder.Property(
                    x => x.GrossAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();


            builder.Property(
                    x => x.DiscountPercent)
                .HasPrecision(
                    9,
                    4)
                .IsRequired();


            builder.Property(
                    x => x.DiscountAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();


            builder.Property(
                    x => x.TaxableAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();

            #endregion


            #region GST

            builder.Property(
                    x => x.GstRate)
                .HasPrecision(
                    9,
                    4)
                .IsRequired();


            builder.Property(
                    x => x.CgstRate)
                .HasPrecision(
                    9,
                    4)
                .IsRequired();


            builder.Property(
                    x => x.SgstRate)
                .HasPrecision(
                    9,
                    4)
                .IsRequired();


            builder.Property(
                    x => x.IgstRate)
                .HasPrecision(
                    9,
                    4)
                .IsRequired();


            builder.Property(
                    x => x.CgstAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();


            builder.Property(
                    x => x.SgstAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();


            builder.Property(
                    x => x.IgstAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();


            builder.Property(
                    x => x.TotalTaxAmount)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();

            #endregion


            #region Line Total

            builder.Property(
                    x => x.LineTotal)
                .HasPrecision(
                    18,
                    2)
                .IsRequired();

            #endregion


            #region Indexes

            builder.HasIndex(
                x => x.InvoiceId);


            builder.HasIndex(
                x => x.DeliveryChallanId);


            builder.HasIndex(
                x => x.DeliveryChallanItemId);


            builder.HasIndex(
                x => x.ItemId);


            builder.HasIndex(
                x => x.CustomerPurchaseOrderItemId);


            builder.HasIndex(
                x => x.ProductionJobId);


            builder.HasIndex(
                x => x.HsnNumber);


            /*
             * Sequence Number must remain unique
             * within one Invoice.
             */
            builder.HasIndex(
                    x => new
                    {
                        x.InvoiceId,
                        x.SequenceNumber
                    })
                .IsUnique();


            /*
             * New Invoice primary source rule.
             *
             * Same Production Job cannot be added
             * twice inside one Invoice.
             *
             * ProductionJobId is nullable only for
             * historical compatibility.
             */
            builder.HasIndex(
                    x => new
                    {
                        x.InvoiceId,
                        x.ProductionJobId
                    })
                .IsUnique()
                .HasFilter(
                    "[ProductionJobId] IS NOT NULL");

            #endregion
        }

        #endregion
    }
}