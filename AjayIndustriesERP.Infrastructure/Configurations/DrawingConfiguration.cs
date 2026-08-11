/*
==============================================================

File : DrawingConfiguration.cs

Purpose :
Configures Drawing revision database mapping.

Final Rules :
- One table stores Drawing revision history.
- DrawingNumber + RevisionNumber is permanently unique.
- Only one Current revision per DrawingNumber.
- Only one Current Drawing per Item.
- Deleted revision numbers remain reserved.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class DrawingConfiguration :
        IEntityTypeConfiguration<Drawing>
    {
        public void Configure(
            EntityTypeBuilder<Drawing> builder)
        {
            #region Table

            builder.ToTable("Drawings");

            builder.HasKey(x =>
                x.DrawingId);

            #endregion

            #region Drawing Identity

            builder.Property(x =>
                    x.DrawingNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x =>
                    x.DrawingName)
                .HasMaxLength(200);

            builder.Property(x =>
                    x.DrawingType)
                .HasMaxLength(100);

            #endregion

            #region Revision

            builder.Property(x =>
                    x.RevisionNumber)
                .HasMaxLength(50)
                .IsRequired();

            /*
             * Revision Number is permanently unique
             * within one Drawing Number.
             *
             * Deleted revisions are intentionally
             * included, therefore revision numbers
             * can never be reused.
             */
            builder.HasIndex(x => new
            {
                x.DrawingNumber,
                x.RevisionNumber
            })
                .IsUnique();

            /*
             * One Drawing Number can contain many
             * historical revisions but maximum
             * one Current revision.
             */
            builder.HasIndex(x =>
                    x.DrawingNumber)
                .IsUnique()
                .HasFilter(
                    "[IsActive] = 1 " +
                    "AND [IsDeleted] = 0");

            #endregion

            #region One Drawing Per Item

            /*
             * Final Business Rule:
             *
             * One Item = One Drawing Number.
             *
             * Historical revisions have
             * IsActive = false.
             *
             * Therefore only one Current Drawing row
             * is allowed for one Item.
             */
            builder.HasIndex(x =>
                    x.ItemId)
                .IsUnique()
                .HasFilter(
                    "[IsActive] = 1 " +
                    "AND [IsDeleted] = 0");

            #endregion

            #region File

            builder.Property(x =>
                    x.FileName)
                .HasMaxLength(255);

            builder.Property(x =>
                    x.FilePath)
                .HasMaxLength(500);

            #endregion

            #region Description

            builder.Property(x =>
                    x.Description)
                .HasMaxLength(500);

            #endregion

            #region Item Relationship

            builder.HasOne(x =>
                    x.Item)
                .WithMany(x =>
                    x.Drawings)
                .HasForeignKey(x =>
                    x.ItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion

            #region Audit

            builder.Property(x =>
                    x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x =>
                    x.ModifiedBy)
                .HasMaxLength(100);

            #endregion
        }
    }
}