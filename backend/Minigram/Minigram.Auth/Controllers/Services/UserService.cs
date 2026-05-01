namespace Minigram.Auth.Services
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Utils.Exceptions;
    using Minigram.Core.ApplicationContext.Repositories;
    using Minigram.Auth.DTO;
    using Minigram.Auth.Models;

    public class UserService
    {
        private readonly IRepository<User> _userRepository;

        private IQueryable<User> Users => _userRepository.Get();

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetByEmail(string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);

            User? user = await Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                throw new EntityNotFoundException(nameof(User), nameof(User.Email), email);
            }

            return user;
        }

        public async Task<User> Create(RegisterRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Check if user with this email already exists
            User? existingUser = await Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            User user = new ()
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                IsEmailVerified = false,
                Password = dto.Password
            };

            await _userRepository.Create(user);
            await _userRepository.SaveAsync();

            return user;
        }
    }
}