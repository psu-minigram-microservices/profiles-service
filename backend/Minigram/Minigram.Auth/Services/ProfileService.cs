namespace Minigram.Auth.Services
{
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.EntityFrameworkCore;
    using Minigram.Auth.Dto;
    using Minigram.Auth.Dto.User;
    using Minigram.Auth.Extensions;
    using Minigram.Core.Exceptions;
    using Minigram.Core.Models;
    using Minigram.Core.Repositories;

    public class ProfileService
    {
        private readonly IRepository<User> _userRepository;

        private IQueryable<User> Users => _userRepository.Get();

        public ProfileService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<ReadProfileDto>> GetAll(QueryParams queryParams)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            IQueryable<User> users = Users;

            int? page = queryParams.Page;
            int? perPage = queryParams.PerPage;

            if (page.HasValue && perPage.HasValue)
            {
                users.Skip(page.Value * perPage.Value).Take(perPage.Value);
            }

            return await users
                .Select(u => u.ToProfileDto())
                .ToListAsync();
        }

        public async Task<ReadProfileDto> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            User? user = await Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user.ToProfileDto();
        }

        public async Task<int> Count()
        {
            return await _userRepository.Count();
        }

        public async Task<User> Update(Guid id, UpdateProfileDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            User? user = await Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            user.Profile.Name = dto.Name;
            user.Profile.PhotoUrl = dto.PhotoUrl;

            await _userRepository.SaveAsync();
            return user;
        }

        public async Task<User> Patch(Guid id, JsonPatchDocument<UpdateProfileDto> patch)
        {
            ArgumentNullException.ThrowIfNull(patch);

            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            User? user = await Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            UpdateProfileDto dto = new ()
            {
                Name = user.Profile.Name,
                PhotoUrl = user.Profile.PhotoUrl,
            };

            patch.ApplyTo(dto);

            user.Profile.Name = dto.Name;
            user.Profile.PhotoUrl = dto.PhotoUrl;

            await _userRepository.SaveAsync();
            return user;
        }
    }
}