using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Models;
using UberSystem.Infrastructure;
using UberSytem.Dto.Requests;
using UberSytem.Dto.Responses;

namespace UberSystem.Api.Customer.Controllers
{
    public class CustomersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ICustomerService _customerService;

        public CustomersController(IUserService userService, IMapper mapper, ICustomerService customerService)
        {
            _userService = userService;
            _mapper = mapper;
            _customerService = customerService;
        }
        
        /// <summary>
        /// Get all customers
        /// </summary>
        /// <returns></returns> 
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("customers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers()
        {
            var cus = await _userService.GetCustomers();
            return _mapper.Map<List<CustomerDTO>>(cus); 
        }

        /// <summary>
        /// Get customer by id
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// </remarks>
        [HttpGet("customers/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
        {
            var customer = await _userService.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound();
            }

            return _mapper.Map<CustomerDTO>(customer);
        }

        /// <summary>
        /// Book trip for customers
        /// </summary>
        /// <param name="bookingRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Book(BookingRequest bookingRequest)
        {
            // Tạo chuyến đi
            var result = await _customerService.CreateTrip(bookingRequest);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error while creating trip");
            }

            // Lấy tài xế trong bán kính 2 km
            var drivers = await _customerService.GetDriversNearby(bookingRequest.sourceLatitude, bookingRequest.sourceLongitude, (float)2.0);

            var driversMap = _mapper.Map<List<DriverDTO>>(drivers);

            return Ok(new { NearbyDrivers = driversMap });
        }

        /// <summary>
        /// Customer feedback on driver
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="tripId"></param>
        /// <param name="rating"></param>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpPost("rate-driver")]
        public async Task<IActionResult> RateDriver(long customerId, long tripId, int rating, string feedback)
        {
            var success = await _customerService.RateDriverAsync(customerId, tripId, rating, feedback);
            if (!success)
                return NotFound(new { message = "Trip not found or not completed." });

            return Ok(new { message = "Driver rated successfully." });
        }

    }
}
