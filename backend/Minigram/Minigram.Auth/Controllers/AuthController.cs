namespace Minigram.Auth.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Minigram.Auth.DTO;

    [ApiVersion("1.0")]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost(nameof(Login))]
        public async Task<JwtResponse> Login(LoginRequestDto dto)
        {
            return new JwtResponse
            {
                AccessToken = string.Empty,
                RefreshToken = "12345",
            };
        }

        [HttpPost(nameof(Register))]
        public async Task<JwtResponse> Register(RegisterRequestDto dto)
        {
            return new JwtResponse
            {
                AccessToken = string.Empty,
                RefreshToken = "12345",
            };
        }

        [HttpPost(nameof(Logout))]
        public async Task<ActionResult> Logout()
        {
            return Ok();
        }

        [HttpPost(nameof(Refresh))]
        public async Task<JwtResponse> Refresh(RefreshRequest dto)
        {
            return new JwtResponse
            {
                AccessToken = string.Empty,
                RefreshToken = "12345",
            };
        }
    }
}
