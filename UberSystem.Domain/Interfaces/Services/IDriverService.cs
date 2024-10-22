using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;

namespace UberSystem.Domain.Interfaces.Services
{
    public interface IDriverService
    {
        Task<bool> StartTripAsync(long tripId, long driverId);
        Task<bool> CompleteTripAsync(long tripId);
        Task<List<Trip>> GetTrips();
    }
}
