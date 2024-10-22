using AutoMapper;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Enums;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;
using UberSytem.Dto.Responses;
namespace UberSystem.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task Add(User user)
        {
            try
            {
                var userRepository = _unitOfWork.Repository<User>();
                var customerRepository = _unitOfWork.Repository<Customer>();
                var driverRepository = _unitOfWork.Repository<Driver>();
                if (user is not null)
                {
                    await _unitOfWork.BeginTransaction();
                    // check duplicate user
                    var existedUser = await userRepository.GetAsync(u => u.Email == user.Email);
                    if (existedUser is not null) throw new Exception("User already exists.");

                //    var userId = await GetLastestUserId();

                    await userRepository.InsertAsync(user);

                    DateTime dateTime = DateTime.UtcNow.AddHours(7);
                    long ticks = dateTime.Ticks;
                    byte[] byteArray = BitConverter.GetBytes(ticks);

                    // add customer or driver into tables
                    if (user.Role == UserRole.CUSTOMER)
                    {
                        var customerId = await GetLastestCusId() + 1;
                        Customer customer = new Customer()
                        {
                            Id = customerId,
                            CreateAt = byteArray,
                            UserId = user.Id,
                        };

                        await customerRepository.InsertAsync(customer);
                    }
                    else if (user.Role == UserRole.DRIVER)
                    {
                        var driverId = await GetLastestDriverId() + 1;
                        Driver driver = new Driver()
                        {
                            Id = driverId,
                            CreateAt = byteArray,
                            UserId = user.Id,
                            LocationLatitude = Random(10.0f, 12.0f),
                            LocationLongitude = Random(100.0f, 110.0f)
                        };
                        await driverRepository.InsertAsync(driver);
                    }
                    await _unitOfWork.CommitTransaction();
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        private double Random(float minValue, float maxValue)
        {
            var random = new Random();
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }


        public Task CheckPasswordAsync(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindByEmail(string email)
        {
            return await _unitOfWork.Repository<User>().FindAsync(email);
        }

        public async Task<User> GetCustomerById(int id)
        {
            var userRepository = _unitOfWork.Repository<User>();
            var users = await userRepository.GetAllAsync();

            var customer = users.FirstOrDefault(u => u.Role == UserRole.CUSTOMER && u.Id == id);

            return customer;
        }

        public async Task<IEnumerable<User>> GetCustomers()
        {
            var userRepository = _unitOfWork.Repository<User>();
            var users = await userRepository.GetAllAsync();

            var customers = users.Where(u => u.Role == (int)UserRole.CUSTOMER);
            return customers;
        }

        public async Task<User> GetDriverById(int id)
        {
            var userRepository = _unitOfWork.Repository<User>();
            var users = await userRepository.GetAllAsync();

            var customer = users.FirstOrDefault(u => u.Role == UserRole.DRIVER && u.Id == id);

            return customer;
        }

        public async Task<IEnumerable<User>> GetDrivers()
        {
            var userRepository = _unitOfWork.Repository<User>();
            var users = await userRepository.GetAllAsync();

            var customers = users.Where(u => u.Role == UserRole.DRIVER);
            return customers;
        }

        public async Task<long> GetLastestCusId()
        {
            var userRepository = _unitOfWork.Repository<Customer>();
            var users = await userRepository.GetAllAsync();

            var customers = users.OrderByDescending(e => e.Id).FirstOrDefault();

            if (customers == null)
            {
                return 1;
            }
            return customers.Id;
        }

        public async Task<long> GetLastestDriverId()
        {
            var userRepository = _unitOfWork.Repository<Driver>();
            var users = await userRepository.GetAllAsync();

            var customers = users.OrderByDescending(e => e.Id).FirstOrDefault();

            if (customers == null)
            {
                return 1;
            }
            return customers.Id;
        }

        public async Task<long> GetLastestUserId()
        {
            var userRepository = _unitOfWork.Repository<User>();
            var users = await userRepository.GetAllAsync();

            var customers = users.OrderByDescending(e => e.Id).FirstOrDefault();

            if (customers == null)
            {
                return 1;
            }
            return customers.Id;
        }

        public async Task<bool> Login(User user)
        {
            try
            {
                await _unitOfWork.BeginTransaction();
                var UserRepos = _unitOfWork.Repository<User>();
                var objUser = await UserRepos.FindAsync(user.Email);
                if (objUser == null)
                    return false;
                if (objUser.Password != user.Password)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> Login(string email, string password)
        {
            var userRepository = _unitOfWork.Repository<User>();
            var users = await userRepository.GetAllAsync();

            var user = users.FirstOrDefault(u => u.Email == email && u.Password == password);
            return user;
        }

        public async Task Update(User user)
        {
            try
            {
                var userRepository = _unitOfWork.Repository<User>();
                var userList = await userRepository.GetAllAsync();
                
                var existedUsername = userList.FirstOrDefault(e => e.UserName == user.UserName && e.Id != user.Id);

                if(existedUsername != null)
                {
                    throw new Exception("Username already existed");
                }

                if (user is not null)
                {
                    await _unitOfWork.BeginTransaction();
                    await userRepository.UpdateAsync(user);
                    await _unitOfWork.CommitTransaction();
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task<User> GetUserById(long id)
        {
            try
            {
                var userRepository = _unitOfWork.Repository<User>();
                var user = await userRepository.FindAsync(id);
                
                return user;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                var userRepository = _unitOfWork.Repository<User>();
                var cusRepository = _unitOfWork.Repository<Customer>();
                var driverRepository = _unitOfWork.Repository<Driver>();


                // Tìm kiếm người dùng với Id tương ứng
                var user = await userRepository.FindAsync(id);
                if (user == null)
                {
                    throw new Exception($"User with ID {id} not found.");
                }

                // Bắt đầu giao dịch xóa
                await _unitOfWork.BeginTransaction();

                if(user.Role == UserRole.CUSTOMER)
                {
                    var cusList = await cusRepository.GetAllAsync();
                    var cus = cusList.FirstOrDefault(e => e.UserId == id);
                    await cusRepository.DeleteAsync(cus);
                }      
                
                if(user.Role == UserRole.DRIVER)
                {
                    var driverList = await driverRepository.GetAllAsync();
                    var driver = driverList.FirstOrDefault(e => e.UserId == id);
                    await driverRepository.DeleteAsync(driver);
                }

                await userRepository.DeleteAsync(user);
                await _unitOfWork.CommitTransaction();
            }
            catch (Exception)
            {
                // Rollback giao dịch nếu có lỗi
                await _unitOfWork.RollbackTransaction();
                throw; // Bắn lại ngoại lệ để tầng trên có thể xử lý lỗi
            }
        }
    }
}

