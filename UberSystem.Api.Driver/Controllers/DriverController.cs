using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UberSystem.Domain.Interfaces.Services;
using UberSytem.Dto.Responses;

namespace UberSystem.Api.Driver.Controllers
{
    public class DriverController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IDriverService _driverService;

        public DriverController(IUserService userService, IMapper mapper, IDriverService driverService)
        {
            _userService = userService;
            _mapper = mapper;
            _driverService = driverService;
        }

        /// <summary>
        /// Get all driver
        /// </summary>
        /// <returns></returns> 
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("drivers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponseModel>>> GetDrivers()
        {
            var cus = await _userService.GetDrivers();
            return _mapper.Map<List<UserResponseModel>>(cus);
        }

        /// <summary>
        /// Get all trips available
        /// </summary>
        /// <returns></returns>
        [HttpGet("trips")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TripDTO>>> GetTrips()
        {
            var cus = await _driverService.GetTrips();
            return _mapper.Map<List<TripDTO>>(cus);
        }

        /// <summary>
        /// Get driver by id
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("drivers/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserResponseModel>> GetDriver(int id)
        {
            var customer = await _userService.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound();
            }

            return _mapper.Map<UserResponseModel>(customer);
        }

        /// <summary>
        /// Driver start a trip
        /// </summary>
        /// <param name="tripId"></param>
        /// <param name="driverId"></param>
        /// <returns></returns>
        [HttpPost("start-trip/{tripId}")]
        public async Task<IActionResult> StartTrip(long tripId, long driverId)
        {
            var success = await _driverService.StartTripAsync(tripId, driverId);
            if (!success)
                return NotFound(new { message = "Trip not found or not in valid state." });

            return Ok(new { message = "Trip started successfully." });
        }

        /// <summary>
        /// Driver complete a trip
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        [HttpPost("complete-trip/{tripId}")]
        public async Task<IActionResult> CompleteTrip(long tripId)
        {
            var success = await _driverService.CompleteTripAsync(tripId);
            if (!success)
                return NotFound(new { message = "Trip not found or not in valid state." });

            return Ok(new { message = "Trip completed successfully." });
        }
    }
}
