using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Enums;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Models;

namespace UberSystem.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> CreateTrip(BookingRequest bookingRequest)
        {
            try
            {
                var cusRepo = _unitOfWork.Repository<Customer>();
                var tripRepo = _unitOfWork.Repository<Trip>();

                var cus = await cusRepo.FindAsync(bookingRequest.customerId);

                if (cus == null)
                {
                    throw new Exception("Customer not found!!!");
                }

                // Validate latitude and longitude
                if (!IsValidLatitude(bookingRequest.sourceLatitude) || !IsValidLongitude(bookingRequest.sourceLongitude))
                {
                    throw new Exception("Invalid source latitude or longitude.");
                }

                if (!IsValidLatitude(bookingRequest.destinationLatitude) || !IsValidLongitude(bookingRequest.destinationLongitude))
                {
                    throw new Exception("Invalid destination latitude or longitude.");
                }

                var trips = await tripRepo.GetAllAsync();
                var tripEntity = trips.OrderByDescending(x => x.Id).FirstOrDefault();
                var tripId = tripEntity.Id + 1;

                var trip = new Trip
                {
                    Id = tripId,
                    Status = TripStatus.Waiting,
                    SourceLatitude = bookingRequest.sourceLatitude,
                    SourceLongitude = bookingRequest.sourceLongitude,
                    DestinationLatitude = bookingRequest.destinationLatitude,
                    DestinationLongitude = bookingRequest.destinationLongitude,
                    CustomerId = bookingRequest.customerId
                };

                await _unitOfWork.BeginTransaction();


                await tripRepo.InsertAsync(trip, true);
           //     await tripRepo.DbContext.SaveChangesAsync();
                await _unitOfWork.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return false;
            }
        }

        public async Task<List<Driver>?> GetDriversNearby(float sourceLatitude, float sourceLongitude, float radius)
        {
            var drivers = await _unitOfWork.Repository<Driver>().GetAllAsync();

            var nearbyDrivers = drivers.Where(driver => CalculateDistance(sourceLatitude, sourceLongitude, (float)driver.LocationLatitude!, (float)driver.LocationLongitude!) <= radius).ToList();

            if (nearbyDrivers.Any())
            {
                var orderedDrivers = await OrderByRatingsDescending(nearbyDrivers);
                return nearbyDrivers;
            }
            else
            {
                return null;
            }

        }

        public async Task<bool> RateDriverAsync(long customerId, long tripId, int rating, string feedback)
        {
            var trip = await _unitOfWork.Repository<Trip>().FindAsync(tripId);

            var ratingRepository = _unitOfWork.Repository<Rating>();

            var ratingList = await ratingRepository.GetAllAsync();

            var lastestRating = ratingList.OrderByDescending(e => e.Id).FirstOrDefault();

            if (trip == null || trip.Status != TripStatus.Finished)
                return false;

            var ratingEntity = new Rating
            {
                Id = lastestRating.Id + 1,
                CustomerId = customerId,
                DriverId = trip.DriverId,
                TripId = tripId,
                Rating1 = rating,
                Feedback = feedback
            };



            await _unitOfWork.Repository<Rating>().InsertAsync(ratingEntity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
        {
            const float R = 6371; 

            float dLat = ToRadians(lat2 - lat1);
            float dLon = ToRadians(lon2 - lon1);

            float a = MathF.Sin(dLat / 2) * MathF.Sin(dLat / 2) +
                       MathF.Cos(ToRadians(lat1)) * MathF.Cos(ToRadians(lat2)) *
                       MathF.Sin(dLon / 2) * MathF.Sin(dLon / 2);
            float c = 2 * MathF.Atan2(MathF.Sqrt(a), MathF.Sqrt(1 - a));

            return R * c; 
        }

        private async Task<List<Driver>> OrderByRatingsDescending(List<Driver> driver)
        {
            var driverIds = driver.Select(d => d.Id).ToList();

            // Fetch all ratings for the nearby drivers

            var allRatings = await _unitOfWork.Repository<Rating>().GetAllAsync();

            var ratings = allRatings
                                  .Where(r => driverIds.Contains((long)r.DriverId))
                                  .ToList();

            // Group ratings by DriverId and calculate average rating
            var driverRatings = ratings
                .GroupBy(r => r.DriverId)
                .Select(g => new
                {
                    DriverId = g.Key,
                    AverageRating = g.Average(r => r.Rating1)
                })
                .ToList();

            // Join drivers with their average ratings and order by rating in descending order
            var orderedDrivers = driver
                .Join(driverRatings,
                    driver => driver.Id,
                    rating => rating.DriverId,
                    (driver, rating) => new { Driver = driver, Rating = rating.AverageRating })
                .OrderByDescending(d => d.Rating)
                .Select(d => d.Driver) // Select the driver entity
                .ToList();

            return orderedDrivers;
        }

        private float ToRadians(float angle)
        {
            return angle * MathF.PI / 180;
        }

        private bool IsValidLatitude(float latitude)
        {
            return latitude >= -90 && latitude <= 90;
        }

        private bool IsValidLongitude(float longitude)
        {
            return longitude >= -180 && longitude <= 180;
        }
    }
}
    
