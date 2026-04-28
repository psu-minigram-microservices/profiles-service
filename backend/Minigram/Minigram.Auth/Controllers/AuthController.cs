namespace Minigram.Auth.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Auth.DTO;
    using Minigram.Auth.Models;
    using Minigram.Auth.Services;
    using Minigram.Core.ApplicationContext.Repositories;
    using Minigram.Core.Utils.Exceptions;

    [ApiVersion("1.0")]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserService _userService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRepository<RefreshSession> _refreshSessionRepository;

        public AuthController(
            TokenService tokenService,
            UserService userService,
            IPasswordHasher<User> passwordHasher,
            IRepository<RefreshSession> refreshSessionRepository)
        {
            _tokenService = tokenService;
            _userService = userService;
            _passwordHasher = passwordHasher;
            _refreshSessionRepository = refreshSessionRepository;
        }

        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Check model state validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new { Message = string.Join("; ", errors) });
            }

            try
            {
                User user = await _userService.GetByEmail(dto.Email);
                
                PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(null!, user.Password, dto.Password);

                if (result == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new { Message = "Invalid email or password." });
                }

                string jwtToken = await _tokenService.Generate(user);
                string refreshToken = Guid.NewGuid().ToString();

                // Create refresh session
                RefreshSession session = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RefreshToken = Guid.Parse(refreshToken),
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                    ExpiresIn = DateTime.UtcNow.AddDays(7), // Refresh tokens valid for 7 days
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    User = user
                };

                await _refreshSessionRepository.Create(session);
                await _refreshSessionRepository.SaveAsync();

                return Ok(new JwtResponse
                {
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken,
                });
            }
            catch (EntityNotFoundException)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }
        }

        [HttpPost(nameof(Register))]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Check model state validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new { Message = string.Join("; ", errors) });
            }

            try
            {
                dto.Password = _passwordHasher.HashPassword(null!, dto.Password);

                User user = await _userService.Create(dto);
                string jwtToken = await _tokenService.Generate(user);
                string refreshToken = Guid.NewGuid().ToString();

                // Create refresh session
                RefreshSession session = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RefreshToken = Guid.Parse(refreshToken),
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                    ExpiresIn = DateTime.UtcNow.AddDays(7), // Refresh tokens valid for 7 days
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    User = user
                };

                await _refreshSessionRepository.Create(session);
                await _refreshSessionRepository.SaveAsync();

                return Ok(new JwtResponse
                {
                    AccessToken = jwtToken,
                    RefreshToken = refreshToken,
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists") || ex.Message.Contains("A user with email"))
            {
                return Conflict(new { Message = "A user with this email already exists." });
            }
        }

        [HttpPost(nameof(Logout))]
        public async Task<ActionResult> Logout()
        {
            // Get the user ID from the claims
            string? userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Ok(); // Nothing to logout
            }

            // Remove all refresh sessions for this user
            List<RefreshSession> userSessions = await _refreshSessionRepository.Get()
                .Where(rs => rs.UserId == userId)
                .ToListAsync();
            
            foreach (RefreshSession session in userSessions)
            {
                _refreshSessionRepository.Delete(session);
            }
            
            await _refreshSessionRepository.SaveAsync();

            return Ok();
        }

        [HttpPost(nameof(Refresh))]
        public async Task<JwtResponse> Refresh(RefreshRequest dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Find the refresh session
            RefreshSession? session = await _refreshSessionRepository.Get()
                .FirstOrDefaultAsync(rs => rs.RefreshToken == Guid.Parse(dto.RefreshToken));

            if (session == null)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            // Check if the refresh token has expired
            if (session.ExpiresIn < DateTime.UtcNow)
            {
                // Remove expired session
                _refreshSessionRepository.Delete(session);
                await _refreshSessionRepository.SaveAsync();
                throw new InvalidOperationException("Refresh token has expired.");
            }

            // Get the user
            User user = await _userService.GetByEmail(session.User.Email);

            // Generate new tokens
            string newAccessToken = await _tokenService.Generate(user);
            string newRefreshToken = Guid.NewGuid().ToString();

            // Update the refresh session
            session.RefreshToken = Guid.Parse(newRefreshToken);
            session.ExpiresIn = DateTime.UtcNow.AddDays(7); // Refresh tokens valid for 7 days
            session.UpdatedAt = DateTime.UtcNow;
            _refreshSessionRepository.Update(session);
            await _refreshSessionRepository.SaveAsync();

            return new JwtResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }
    }
}
