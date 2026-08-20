/*
============================================================
File: ItemProcessRoutingConfiguration.cs

Purpose:
Configures ItemProcessRouting for Entity Framework Core.

Responsibilities:
- Map Routing Header to database.
- Configure Item relationship.
- Configure Routing Code.
- Configure Revision and Status.
- Prevent duplicate Revision Number for the same Item.
- Configure Routing Step relationship.
- Configure query indexes.

Important:
- Routing Code is globally unique.
- Item + RevisionNumber is unique.
- Old revisions are never recreated using the same revision.
- Only one Released revision per Item will be enforced by
  Application Service during Release workflow.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ItemProcessRoutingConfiguration
        : IEntityTypeConfiguration<ItemProcessRouting>
    {
        public void Configure(
            EntityTypeBuilder<ItemProcessRouting> builder)
        {
            #region Table

            builder.ToTable(
                "ItemProcessRoutings");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Routing Code

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();

            #endregion


            #region Item Relationship

            builder.Property(x =>
                    x.ItemId)
                .IsRequired();


            builder.HasOne(x =>
                    x.Item)
                .WithMany()
                .HasForeignKey(x =>
                    x.ItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Revision Information

            builder.Property(x =>
                    x.RevisionNumber)
                .IsRequired();


            builder.Property(x =>
                    x.Status)
                .IsRequired();


            /*
             * Revision Number must never repeat for the same Item.
             *
             * Deleted records are intentionally included because
             * historical Routing revisions must not be reused.
             */

            builder.HasIndex(x =>
                    new
                    {
                        x.ItemId,
                        x.RevisionNumber
                    })
                .IsUnique();

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Routing Steps

            builder.HasMany(x =>
                    x.Steps)
                .WithOne(x =>
                    x.ItemProcessRouting)
                .HasForeignKey(x =>
                    x.ItemProcessRoutingId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.ItemId);


            builder.HasIndex(x =>
                x.Status);


            builder.HasIndex(x =>
                x.EffectiveFrom);


            builder.HasIndex(x =>
                x.IsActive);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}