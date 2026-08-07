/*
==============================================================

File : IItemSpecificationRepository.cs

Purpose :
Defines database operations for Item Specification rows.

Notes :
- Item Specifications are child records of Item Master.
- Removed rows are soft deleted.
- SaveChanges is intentionally separate so Item and its
  Specifications can be saved together in one DbContext unit.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines persistence operations for Item Specifications.
    /// </summary>
    public interface IItemSpecificationRepository
    {
        #region Read Operations

        Task<List<ItemSpecification>> GetByItemIdAsync(
            int itemId);

        Task<ItemSpecification?> GetByIdAsync(
            int itemSpecificationId);

        #endregion

        #region Write Operations

        Task AddAsync(
            ItemSpecification itemSpecification);

        Task AddRangeAsync(
            IEnumerable<ItemSpecification> itemSpecifications);

        Task UpdateAsync(
            ItemSpecification itemSpecification);

        Task SoftDeleteAsync(
            ItemSpecification itemSpecification);

        Task SoftDeleteRangeAsync(
            IEnumerable<ItemSpecification> itemSpecifications);

        #endregion

        #region Duplicate Validation

        Task<bool> ExistsAsync(
            int itemId,
            int specificationId);

        Task<bool> ExistsAsync(
            int itemId,
            int specificationId,
            int itemSpecificationId);

        #endregion

        #region Save Changes

        Task SaveChangesAsync();

        #endregion
    }
}