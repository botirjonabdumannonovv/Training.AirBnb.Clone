﻿using Backend_Project.Application.Entity;
using Backend_Project.Domain.Entities;
using Backend_Project.Domain.Exceptions.EntityExceptions;
using Backend_Project.Persistence.DataContexts;
using System.Linq.Expressions;

namespace Backend_Project.Infrastructure.Services.ListingServices;
public class DescriptionService : IEntityBaseService<Description>
{
    private readonly IDataContext _dataContext;

    public DescriptionService(IDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async ValueTask<Description> CreateAsync(Description description, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        ValidateDescription(description);
        await _dataContext.Descriptions.AddAsync(description, cancellationToken);

        if (saveChanges)
            await _dataContext.SaveChangesAsync();

        return description;
    }

    public async ValueTask<Description> DeleteAsync(Guid id, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        var removedListingDescription = await GetByIdAsync(id, cancellationToken);

        await _dataContext.Descriptions.RemoveAsync(removedListingDescription, cancellationToken);
        
        if (saveChanges) await _dataContext.SaveChangesAsync();
        
        return removedListingDescription;
    }

    public async ValueTask<Description> DeleteAsync(Description entity, bool saveChanges = true, CancellationToken cancellationToken = default)
         => await DeleteAsync(entity.Id, saveChanges, cancellationToken);

    public IQueryable<Description> Get(Expression<Func<Description, bool>> predicate)
        => GetUndeletedListingDescription().Where(predicate.Compile()).AsQueryable();

    public ValueTask<ICollection<Description>> GetAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
         => new ValueTask<ICollection<Description>>(GetUndeletedListingDescription()
             .Where(description => ids
                .Contains(description.Id))
             .ToList());

    public ValueTask<Description> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
         => new ValueTask<Description>(GetUndeletedListingDescription()
             .FirstOrDefault(description => description.Id.Equals(id))
             ?? throw new EntityNotFoundException<Description>("Description not found."));

    public async ValueTask<Description> UpdateAsync(Description entity, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        ValidateDescription(entity);
        var foundlistingdescription = await GetByIdAsync(entity.Id);

        foundlistingdescription.ListingDescription = entity.ListingDescription;
        foundlistingdescription.TheSpace = entity.TheSpace;
        foundlistingdescription.OtherDetails = entity.OtherDetails;
        foundlistingdescription.InteractionWithGuests = entity.InteractionWithGuests;

        await _dataContext.Descriptions.UpdateAsync(foundlistingdescription);

        if (saveChanges) await _dataContext.SaveChangesAsync();

        return foundlistingdescription;
    }
    private bool ValidateDescription(Description description)
    {
        if (description.ListingDescription.Length > 500 | string
            .IsNullOrWhiteSpace(description.ListingDescription))
            return false;

        if (string.IsNullOrWhiteSpace(description.TheSpace))
            return false;

        if (string.IsNullOrWhiteSpace(description.GuestAccess))
            return false;

        if (string.IsNullOrWhiteSpace(description.OtherDetails))
            return false;

        if (string.IsNullOrWhiteSpace(description.InteractionWithGuests))
            return false;

        return true;
    }
    private IQueryable<Description> GetUndeletedListingDescription() => _dataContext
        .Descriptions.Where(res => !res.IsDeleted).AsQueryable();
}
