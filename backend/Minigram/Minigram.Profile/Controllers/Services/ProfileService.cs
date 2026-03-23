namespace Minigram.Profile.Controllers.Services
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Dto;
    using Minigram.Core.Utils.Exceptions;
    using Minigram.Core.ApplicationContext.Repositories;
    using Minigram.Profile.Extensions;
    using Minigram.Profile.Controllers.Dto;
    using Minigram.Profile.ApplicationContext.Models;
    using Minigram.Core.Utils;


    public class ProfileService
    {
        private readonly IRepository<Profile> _profileRepository;

        private IQueryable<Profile> Profiles => _profileRepository.Get();

        public ProfileService(IRepository<Profile> profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<List<ProfileResponseDto>> GetAll(QueryParams queryParams)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            IQueryable<Profile> profiles = Profiles;

            int? page = queryParams.Page;
            int? perPage = queryParams.PerPage;

            if (page.HasValue && perPage.HasValue)
            {
                profiles.Skip(page.Value * perPage.Value).Take(perPage.Value);
            }

            return await profiles
                .Select(u => u.ToDto())
                .ToListAsync();
        }

        public async Task<Profile> Get(Guid id)
        {
            Assertions.ThrowIfNullOrEmpty(id, nameof(id));

            Profile? profile = await Profiles
                .FirstOrDefaultAsync(u => u.Id == id);

            if (profile == null)
            {
                throw new EntityNotFoundException(typeof(Profile), id);
            }

            return profile;
        }

        public async Task<Profile> GetByUserId(Guid userId)
        {
            Assertions.ThrowIfNullOrEmpty(userId, nameof(userId));

            Profile? profile = await Profiles
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (profile == null)
            {
                throw new EntityNotFoundException(nameof(Profile), nameof(Profile.UserId), userId);
            }

            return profile;
        }

        public async Task<int> Count()
        {
            return await _profileRepository.Count();
        }

        public async Task<Profile> Create(Guid userId, ProfileRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            Assertions.ThrowIfNullOrEmpty(userId, nameof(userId));

            Profile profile = new ()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                PhotoUrl = dto.PhotoUrl,
            };

            await _profileRepository.Create(profile);
            await _profileRepository.SaveAsync();

            return profile;
        }

        public async Task Update(Profile profile, ProfileRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(profile);

            profile.Name = dto.Name;
            profile.PhotoUrl = dto.PhotoUrl;

            _profileRepository.Update(profile);
            await _profileRepository.SaveAsync();
        }
    }
}