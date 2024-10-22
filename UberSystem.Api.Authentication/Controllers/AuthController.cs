using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Enums;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Service;
using UberSytem.Dto;
using UberSytem.Dto.Requests;
using UberSytem.Dto.Responses;

namespace UberSystem.Api.Authentication.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;

        public AuthController(IUserService userService, TokenService tokenService, IMapper mapper, MailService mailService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _mapper = mapper;
            _mailService = mailService;
        }

        /// <summary>
        /// Login to the system
        /// </summary>
        /// <param name="request"></param>
        /// 
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseModel<UserResponseModel>>> Login([FromBody] LoginModel request)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = await _userService.Login(request.Email, request.Password);
            if (result is null) return NotFound(new ApiResponseModel<string>
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "User not found"
            });
            var accessToken = _tokenService.GenerateToken(result, new List<string> { result.Role.ToString() });
            var response = _mapper.Map<UserResponseModel>(result);
            response.AccessToken = accessToken;

            return Ok(new ApiResponseModel<UserResponseModel>
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Success",
                Data = response
            });
        }

        /// <summary>
        /// Sign up into Uber System
        /// </summary>
        /// <param name = "request" ></param >
        [HttpPost("sign-up")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponseModel<string>>> Signup([FromBody] SignupModel request)
        {
            if (!ModelState.IsValid) return BadRequest();
            // Authenticate for role
            if (request.Role != "CUSTOMER" && request.Role != "DRIVER")
                return BadRequest(new ApiResponseModel<string>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid role's value in the system!"
                });
            var user = _mapper.Map<User>(request);

            user.Id = await _userService.GetLastestUserId() + 1;

            await _userService.Add(user);
            return Ok(new ApiResponseModel<string>
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Success",
            });
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update(long id, string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || username.Contains(" "))
                {
                    return BadRequest("Invalid email");
                }

                if (string.IsNullOrWhiteSpace(password) || password.Contains(" "))
                {
                    return BadRequest("Invalid password");
                } 
                
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return BadRequest("User not found");
                }

                user.UserName = username;
                user.Password = password;

                await _userService.Update(user);

                return Ok("Update successfully");   

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var user = await _userService.GetUserById(id);

                if(user == null)
                {
                    return BadRequest("User not found");
                } 

                await _userService.Delete(id);

                return Ok("Delete successfully");

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Verify email 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCodeRequest request)
        {
            try
            {
                var isVerified = await _mailService.VerifyEmailAsync(request.Email, request.Code);
                if (!isVerified)
                    return BadRequest("Invalid verification code or it has expired.");

                return Ok("Email verification successful.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Send code to verify
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode(string email)
        {
            try
            {
                await _mailService.SendVerificationCodeAsync(email);
                return Ok("The verification code has sent to your email!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
    }

    public class VerifyCodeRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}

