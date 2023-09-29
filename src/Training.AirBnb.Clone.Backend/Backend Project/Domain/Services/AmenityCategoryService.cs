﻿using Backend_Project.Domain.Entities;
using Backend_Project.Domain.Exceptions.AmentyCategoryException;
using Backend_Project.Domain.Interfaces;
using Backend_Project.Persistance.DataContexts;
using System.Linq.Expressions;

namespace Backend_Project.Domain.Services
{
    public class AmenityCategoryService : IEntityBaseService<AmenityCategory>
    {
        private readonly IDataContext _appDataContext;

        public AmenityCategoryService(IDataContext appDataContext)
        {
            _appDataContext = appDataContext;
        }

        public async ValueTask<AmenityCategory> CreateAsync(AmenityCategory amenityCategory, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            if (!IsValidCategoryName(amenityCategory.CategoryName))
                throw new AmenityCategoryFormatException("Invalid categoryName!");

            await _appDataContext.AmenityCategories.AddAsync(amenityCategory, cancellationToken);

            if (saveChanges)
                await _appDataContext.AmenityCategories.SaveChangesAsync();

            return amenityCategory;
        }

        public async ValueTask<AmenityCategory> DeleteAsync(Guid id, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var deletedAmenityCategory = await GetByIdAsync(id);

            if (deletedAmenityCategory is null)
                throw new AmenityCategoryNotFoundException("AmentyCategory not found!");

            deletedAmenityCategory.IsDeleted = true;
            deletedAmenityCategory.DeletedDate = DateTimeOffset.UtcNow;

            if (saveChanges)
                await _appDataContext.AmenityCategories.SaveChangesAsync();

            return deletedAmenityCategory;
        }

        public async ValueTask<AmenityCategory> DeleteAsync(AmenityCategory amenityCategory, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var deletedAmenityCategory = await GetByIdAsync(amenityCategory.Id);

            if (deletedAmenityCategory is null)
                throw new AmenityCategoryNotFoundException("AmentyCategory not found!");

            deletedAmenityCategory.IsDeleted = true;
            deletedAmenityCategory.DeletedDate = DateTimeOffset.UtcNow;

            if (saveChanges)
                await _appDataContext.AmenityCategories.SaveChangesAsync();

            return deletedAmenityCategory;
        }

        public IQueryable<AmenityCategory> Get(Expression<Func<AmenityCategory, bool>> predicate)
        {
            return GetUndeletedAmentyCategories().Where(predicate.Compile()).AsQueryable();
        }

        public ValueTask<ICollection<AmenityCategory>> GetAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var amenityCategories = GetUndeletedAmentyCategories().
                Where(amenityCategory =>  ids.Contains(amenityCategory.Id));

            return new ValueTask<ICollection<AmenityCategory>>(amenityCategories.ToList());
        }

        public ValueTask<AmenityCategory> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return new ValueTask<AmenityCategory>(GetUndeletedAmentyCategories().
                FirstOrDefault(amenityCategory => amenityCategory.Id == id) ??
                throw new AmenityCategoryNotFoundException("AmentyCategory not found!"));
        }

        public async ValueTask<AmenityCategory> UpdateAsync(AmenityCategory amenityCategory, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var updatedAmenityCategory = await GetByIdAsync(amenityCategory.Id);

            if (updatedAmenityCategory is null)
                throw new AmenityCategoryNotFoundException("AmentyCategory not found!");
            if (!IsValidCategoryName(amenityCategory.CategoryName))
                throw new AmenityCategoryFormatException("Invalid categoryName!");

            updatedAmenityCategory.CategoryName = amenityCategory.CategoryName;
            updatedAmenityCategory.ModifiedDate = DateTimeOffset.UtcNow;

            if (saveChanges)
                await _appDataContext.AmenityCategories.SaveChangesAsync();

            return updatedAmenityCategory;
        }

        private bool IsValidCategoryName(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
                return true;
            else 
                return false;
        }

        private IQueryable<AmenityCategory> GetUndeletedAmentyCategories() => _appDataContext.AmenityCategories.
            Where(amenityCategory => !amenityCategory.IsDeleted).AsQueryable();
    }
}
