using Backend_Project.Domain.Entities;
using System.Linq.Expressions;
using Backend_Project.Persistance.DataContexts;
using Backend_Project.Domain.Exceptions.ReservationExeptions;
using Backend_Project.Application.Interfaces;

namespace Backend_Project.Infrastructure.Services.ReservationServices
{
    public class ReservationService : IEntityBaseService<Reservation>
    {
        private readonly IDataContext _appDataContext;

        public ReservationService(IDataContext appDateContext)
        {
            _appDataContext = appDateContext;
        }

        public async ValueTask<Reservation> CreateAsync(Reservation reservation, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            if (GetUndelatedReservations().Any(x => x.Equals(reservation)))
                throw new ReservationAlreadyExistsException("This reservation already exists");
            if (IsValidEntity(reservation))
                await _appDataContext.Reservations.AddAsync(reservation, cancellationToken);
            else
                throw new ReservationValidationException("Reservation didn't pass validation");
            if (saveChanges)
                await _appDataContext.Reservations.SaveChangesAsync();
            return reservation;
        }

        public async ValueTask<Reservation> DeleteAsync(Guid id, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var removedReservation = await GetByIdAsync(id);
            removedReservation.IsDeleted = true;
            removedReservation.DeletedDate = DateTimeOffset.UtcNow;
            if (saveChanges)
                await _appDataContext.Reservations.SaveChangesAsync();
            return removedReservation;
        }

        public async ValueTask<Reservation> DeleteAsync(Reservation reservation, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var removedReservation = await GetByIdAsync(reservation.Id);
            removedReservation.IsDeleted = true;
            removedReservation.DeletedDate = DateTimeOffset.UtcNow;
            if (saveChanges)
                await _appDataContext.Reservations.SaveChangesAsync();
            return removedReservation;
        }

        public IQueryable<Reservation> Get(Expression<Func<Reservation, bool>> predicate)
        {
            return GetUndelatedReservations().Where(predicate.Compile()).AsQueryable();
        }

        public ValueTask<ICollection<Reservation>> GetAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var reservation = GetUndelatedReservations()
                .Where(reservation => ids.Contains(reservation.Id));
            return new ValueTask<ICollection<Reservation>>(reservation.ToList());
        }

        public ValueTask<Reservation> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reservation = GetUndelatedReservations().FirstOrDefault(reservation => reservation.Id.Equals(id));
            if (reservation is null)
                throw new ReservationNotFound("Reservation not found.");
            return new ValueTask<Reservation>(reservation);
        }

        public async ValueTask<Reservation> UpdateAsync(Reservation reservation, bool saveChanges = true, CancellationToken cancellationToken = default)
        {
            var foundReseervation = GetUndelatedReservations().FirstOrDefault(reservation =>
                reservation.Id.Equals(reservation.Id));
            if (foundReseervation is null)
                throw new ReservationNotFound("Reservation not found.");
            if (!IsValidEntity(reservation))
                throw new ReservationValidationException("Reservation is not valid.");

            foundReseervation.ListingId = reservation.ListingId;
            foundReseervation.BookedBy = reservation.BookedBy;
            foundReseervation.OccupancyId = reservation.OccupancyId;
            foundReseervation.StartDate = reservation.StartDate;
            foundReseervation.EndDate = reservation.EndDate;
            foundReseervation.TotalPrice = reservation.TotalPrice;
            foundReseervation.ModifiedDate = DateTimeOffset.UtcNow;
            if (saveChanges)
                await _appDataContext.Reservations.SaveChangesAsync();
            return foundReseervation;
        }

        private bool IsValidEntity(Reservation reservation)
        {
            if (reservation.ListingId.Equals(default))
                return false;
            if (reservation.BookedBy.Equals(default))
                return false;
            if (reservation.OccupancyId.Equals(default))
                return false;
            if (reservation.StartDate.Equals(default)
                || !IsValidDateStartDate(reservation.StartDate))
                return false;
            if (reservation.EndDate.Equals(default)
                || IsValidDateEndDate(reservation.StartDate, reservation.EndDate))
                return false;
            if (reservation.TotalPrice <= 0)
                return false;
            return true;

        }

        private bool IsValidDateStartDate(DateTime startdate)
        {
            if (startdate <= DateTime.UtcNow)
                return false;
            return true;
        }
        private bool IsValidDateEndDate(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
                return false;
            if (endDate <= DateTime.UtcNow)
                return false;
            return true;
        }
        private IQueryable<Reservation> GetUndelatedReservations() => _appDataContext.Reservations
            .Where(res => !res.IsDeleted).AsQueryable();
    }
}