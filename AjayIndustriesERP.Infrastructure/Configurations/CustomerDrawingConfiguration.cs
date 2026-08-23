/*
==============================================================

File : CustomerDrawingConfiguration.cs

Purpose :
Configures Customer Drawing entity mapping.

Final Design :
- Customer Drawing follows Drawing Master revision workflow.
- One Customer + One Item = One Drawing Number.
- One Customer Drawing has many revision rows.
- Only one revision can be Current / Active.
- Revision Numbers are never reused.
- Same Drawing Number may exist for different Customers.
- Different Customers may have Drawings for the same Item.
- Existing Drawing Master remains completely separate.

Important Rules :
- Historical revision rows remain in CustomerDrawings table.
- Current revision = IsActive = true and IsDeleted = false.
- Deleted revisions remain preserved.
- Complete Drawing delete is handled as Soft Delete.
- Physical files are not controlled by EF configuration.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class CustomerDrawingConfiguration :
        IEntityTypeConfiguration<CustomerDrawing>
    {
        public void Configure(
            EntityTypeBuilder<CustomerDrawing> builder)
        {
            #region Table / Key

            builder.ToTable(
                "CustomerDrawings");

            builder.HasKey(
                x => x.CustomerDrawingId);

            #endregion


            #region Customer / Item

            builder.Property(
                    x => x.CustomerId)
                .IsRequired();

            builder.Property(
                    x => x.ItemId)
                .IsRequired();

            #endregion


            #region Drawing Information

            builder.Property(
                    x => x.DrawingNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(
                    x => x.DrawingName)
                .HasMaxLength(200);

            builder.Property(
                    x => x.DrawingType)
                .HasMaxLength(100);

            builder.Property(
                    x => x.RevisionNumber)
                .HasMaxLength(50)
                .IsRequired();

            #endregion


            #region File Information

            builder.Property(
                    x => x.FileName)
                .HasMaxLength(255);

            builder.Property(
                    x => x.FilePath)
                .HasMaxLength(500);

            builder.Property(
                    x => x.Description)
                .HasMaxLength(500);

            #endregion


            #region Revision Uniqueness

            /*
             * A Revision Number can never be reused
             * for the same Customer Drawing Number.
             *
             * Example:
             *
             * Customer A
             * DRG-100
             * RV-01
             * RV-02
             *
             * Even if RV-01 is soft deleted,
             * RV-01 cannot be created again.
             *
             * Another Customer may independently have:
             *
             * Customer B
             * DRG-100
             * RV-01
             */
            builder.HasIndex(
                    x => new
                    {
                        x.CustomerId,
                        x.DrawingNumber,
                        x.RevisionNumber
                    })
                .IsUnique();

            #endregion


            #region Current Drawing Number Uniqueness

            /*
             * Only one Current revision can exist
             * for a Drawing Number within one Customer.
             *
             * Historical revisions use IsActive = false,
             * therefore they do not participate in
             * this filtered unique index.
             */
            builder.HasIndex(
                    x => new
                    {
                        x.CustomerId,
                        x.DrawingNumber
                    })
                .IsUnique()
                .HasFilter(
                    "[IsActive] = 1 AND [IsDeleted] = 0");

            #endregion


            #region Current Customer + Item Uniqueness

            /*
             * One Customer + One Item =
             * One current Customer Drawing.
             *
             * IMPORTANT:
             *
             * This index is filtered by IsActive.
             *
             * Therefore revision history such as:
             *
             * RV-01 => IsActive = false
             * RV-02 => IsActive = true
             *
             * can exist together.
             *
             * The previous configuration used only:
             *
             * [IsDeleted] = 0
             *
             * which incorrectly prevented multiple
             * revision rows.
             */
            builder.HasIndex(
                    x => new
                    {
                        x.CustomerId,
                        x.ItemId
                    })
                .IsUnique()
                .HasFilter(
                    "[IsActive] = 1 AND [IsDeleted] = 0");

            #endregion


            #region Relationships

            builder.HasOne(
                    x => x.Customer)
                .WithMany()
                .HasForeignKey(
                    x => x.CustomerId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            builder.HasOne(
                    x => x.Item)
                .WithMany()
                .HasForeignKey(
                    x => x.ItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Audit

            builder.Property(
                    x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(
                    x => x.ModifiedBy)
                .HasMaxLength(100);

            #endregion
        }
    }
}