using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Models;

namespace UberSystem.Domain.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<List<Driver>?> GetDriversNearby(float latitude, float longitude, float radius);
        Task<bool> CreateTrip(BookingRequest bookingRequest);
        Task<bool> RateDriverAsync(long customerId, long tripId, int rating, string feedback);
    }
}
