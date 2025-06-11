using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;


    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }


    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("Username is required");
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required");

            var success = await _authService.RegisterUser(request);
            /*return success ?
                Ok(new { message = "Registration successful" }) :
                BadRequest("Username already exists");*/
            if (success)
            {
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Message = "Registration successful"
                });
            }

            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = "Username already exists"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            Console.WriteLine($"request:{request.Username},{request.Password}");
            var user = await _authService.AuthenticateUser(request);
            
            if (user != null)
            {
            Console.WriteLine($"email:{user.Email}");
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Message = "login successful",
                    Data = user  //包含完整的用户信息//
                });
            }
            else
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Login Failed!wrong username or password!"
                });
            }
        }
    }
