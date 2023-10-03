﻿using Backend_Project.Application.Interfaces;
using Backend_Project.Domain.Entities;
using Backend_Project.Domain.Exceptions.EntityExceptions;
using Backend_Project.Persistance.DataContexts;
using System.Linq.Expressions;

namespace Backend_Project.Infrastructure.Services.ListingServices;

public class ListingService : IEntityBaseService<Listing>
{ 
    private readonly IDataContext _appDataContext;

    public ListingService(IDataContext appDataContext)
    {
        _appDataContext = appDataContext;
    }

    public async ValueTask<Listing> CreateAsync(Listing listing, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        if (!IsValidListing(listing))
            throw new EntityValidationException<Listing>("Listing did not pass validation.");

        await _appDataContext.Listings.AddAsync(listing, cancellationToken);

        if (saveChanges) await _appDataContext.Listings.SaveChangesAsync(cancellationToken);

        return listing;
    }

    public ValueTask<ICollection<Listing>> GetAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => new ValueTask<ICollection<Listing>>(GetUndeletedListings()
            .Where(listing => ids.Contains(listing.Id))
            .ToList());

    public ValueTask<Listing> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => new ValueTask<Listing>(GetUndeletedListings()
                .FirstOrDefault(listing => listing.Id == id)
                ?? throw new EntityNotFoundException<Listing>("Listing not found."));

    public IQueryable<Listing> Get(Expression<Func<Listing, bool>> predicate)
        => GetUndeletedListings().Where(predicate.Compile()).AsQueryable();

    public async ValueTask<Listing> UpdateAsync(Listing listing, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        if (!IsValidListing(listing))
            throw new EntityValidationException<Listing>("Listing did not pass validation.");

        var foundListing = await GetByIdAsync(listing.Id, cancellationToken);

        foundListing.Title = listing.Title;
        foundListing.Description = listing.Description;
        foundListing.Status = listing.Status;
        foundListing.OccupancyId = listing.OccupancyId;
        foundListing.Price = listing.Price;
        foundListing.ModifiedDate = DateTime.UtcNow;
        
        if (saveChanges) await _appDataContext.Listings.SaveChangesAsync(cancellationToken);

        return listing;
    }

    public async ValueTask<Listing> DeleteAsync(Guid id, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        var foundListing = await GetByIdAsync(id, cancellationToken);

        foundListing.IsDeleted = true;
        foundListing.DeletedDate = DateTime.UtcNow;

        if (saveChanges) await _appDataContext.Listings.SaveChangesAsync(cancellationToken);

        return foundListing;
    }

    public async ValueTask<Listing> DeleteAsync(Listing listing, bool saveChanges = true, CancellationToken cancellationToken = default)
        => await DeleteAsync(listing.Id, saveChanges, cancellationToken);

    private bool IsValidListing(Listing listing)
        => (!string.IsNullOrWhiteSpace(listing.Title) && listing.Title.Length > 2 && listing.Title.Length <= 50)
            && (!string.IsNullOrWhiteSpace(listing.Description) && listing.Description.Length > 2 && listing.Description.Length <= 4000)
            && listing.Price > 0;

    private IQueryable<Listing> GetUndeletedListings()
        => _appDataContext.Listings.Where(listing => !listing.IsDeleted).AsQueryable();
}