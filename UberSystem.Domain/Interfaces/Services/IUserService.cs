using System.ComponentModel;
using UberSystem.Domain.Entities;

namespace UberSystem.Domain.Interfaces.Services
{
    public interface IUserService
	{
        Task<User> FindByEmail(string  email);
        Task<User> GetUserById(long id);
        Task Update(User user);
        Task Add(User user);
        Task Delete(long id);
        Task<bool> Login(User user);
        Task CheckPasswordAsync(User user);
        Task<IEnumerable<User>> GetCustomers();
        Task<User> GetCustomerById(int id);        
        Task<IEnumerable<User>> GetDrivers();
        Task<User> GetDriverById(int id);
        Task<User?> Login(string email, string password);
        Task<long> GetLastestUserId();
        Task<long> GetLastestCusId();
        Task<long> GetLastestDriverId();
    }
}

