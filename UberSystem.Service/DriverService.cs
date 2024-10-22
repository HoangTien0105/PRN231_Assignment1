using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Enums;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;

namespace UberSystem.Service
{
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DriverService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> StartTripAsync(long tripId, long driverId)
        {
            var trip = await _unitOfWork.Repository<Trip>().FindAsync(tripId);
            if (trip == null || trip.Status != Domain.Enums.TripStatus.Waiting)
                return false;

            trip.DriverId = driverId;

            trip.Status = Domain.Enums.TripStatus.Ongoing;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }


        public async Task<bool> CompleteTripAsync(long tripId)
        {
            var trip = await _unitOfWork.Repository<Trip>().FindAsync(tripId);
            if (trip == null || trip.Status != Domain.Enums.TripStatus.Ongoing)
                return false;

            trip.Status = Domain.Enums.TripStatus.Finished;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<Trip>> GetTrips()
        {
            var userRepository = _unitOfWork.Repository<Trip>();
            var users = await userRepository.GetAllAsync();

            var customers = users.Where(u => u.Status == (int)TripStatus.Waiting).ToList();
            return customers;
        }
    }
}
