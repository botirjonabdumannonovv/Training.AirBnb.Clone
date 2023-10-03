﻿using Backend_Project.Application.Interfaces;
using Backend_Project.Domain.Entities;
using Backend_Project.Domain.Exceptions.CityExceptions;
using Backend_Project.Persistance.DataContexts;
using System.Linq.Expressions;

namespace Backend_Project.Infrastructure.Services.LocationServices
{
    public class CityService : IEntityBaseService<City>
    {
        private IDataContext _appDataContext;
        public CityService(IDataContext dataContext)
        {
            _appDataContext = dataContext;
        }

        public async ValueTask<City> CreateAsync(City city, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            if (IsUnique(city))
                throw new CityAlreadyExistsException("This City already Exists");
            if (!IsValidCityName(city))
                throw new CityFormatException("The city is in the wrong format");
            await _appDataContext.Cities.AddAsync(city, cancellationToken);
            if (saveChanges)
                await _appDataContext.Cities.SaveChangesAsync();
            return city;
        }

        public async ValueTask<City> DeleteAsync(Guid id, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var deletedCity = await GetByIdAsync(id);
            deletedCity.DeletedDate = DateTimeOffset.UtcNow;
            deletedCity.IsDeleted = true;
            if (saveChanges)
                await _appDataContext.Cities.SaveChangesAsync();
            return deletedCity;
        }

        public async ValueTask<City> DeleteAsync(City city, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var deletedCity = await GetByIdAsync(city.Id);
            deletedCity.DeletedDate = DateTimeOffset.UtcNow;
            deletedCity.IsDeleted = true;
            if (saveChanges)
                await _appDataContext.Cities.SaveChangesAsync();
            return deletedCity;
        }

        public IQueryable<City> Get(Expression<Func<City, bool>> predicate)
        {
            return GetUndeletedCities().Where(predicate.Compile()).AsQueryable();
        }

        public ValueTask<ICollection<City>> GetAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var cities = GetUndeletedCities()
            .Where(city => ids.Contains(city.Id));
            return new ValueTask<ICollection<City>>(cities.ToList());
        }

        public ValueTask<City> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var foundCity = GetUndeletedCities().FirstOrDefault(city => city.Id.Equals(id));
            if (foundCity is null)
                throw new CityNotFoundException("City not found");
            return new ValueTask<City>(foundCity);
        }

        public async ValueTask<City> UpdateAsync(City city, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var foundCity = await GetByIdAsync(city.Id);
            if (!IsValidCityName(city))
                throw new CityFormatException("The city is in the wrong format");
            foundCity.ModifiedDate = DateTimeOffset.UtcNow;
            foundCity.Name = city.Name;
            if (saveChanges)
                await _appDataContext.Cities.SaveChangesAsync();
            return foundCity;
        }

        private IQueryable<City> GetUndeletedCities() => _appDataContext.Cities
            .Where(city => !city.IsDeleted).AsQueryable();

        private bool IsValidCityName(City city)
        {
            if (string.IsNullOrWhiteSpace(city.Name)
                || city.Name.Length <= 4
                || city.Name.Length > 185
                || city.CountryId.Equals(default))
                return false;
            return true;
        }

        private bool IsUnique(City city) => !GetUndeletedCities().Any(c => c.Name.Equals(city.Name));
    }
}