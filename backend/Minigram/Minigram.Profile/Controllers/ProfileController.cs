namespace Minigram.Profile.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Minigram.Profile.Extensions;
    using Minigram.Profile.Controllers.Dto;
    using Minigram.Profile.Controllers.Services;
    using Minigram.Profile.ApplicationContext.Models;
    using Minigram.Core.Dto;

    [ApiVersion("1.0")]
    [ApiController]
    [Route("[controller]s")]
    public class ProfileController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;

        private readonly ProfileService _profileService;

        private Guid UserId =>
            _currentUserService.UserGuid ?? throw new UnauthorizedAccessException();

        public ProfileController(
            CurrentUserService currentUserService,
            ProfileService profileService)
        {
            _currentUserService = currentUserService;
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<PagedResponse<ProfileResponseDto>> GetAll([FromQuery] QueryParams queryParams)
        {
            int count = await _profileService.Count();
            List<ProfileResponseDto> data = await _profileService.GetAll(queryParams);

            return new PagedResponse<ProfileResponseDto>
            {
                Count = count,
                Data = data,
            };
        }

        [HttpGet(nameof(Me))]
        public async Task<ProfileResponseDto> Me()
        {
            Profile profile = await _profileService.GetByUserId(UserId);
            return profile.ToDto();
        }

        [HttpGet($"{{{nameof(id)}}}")]
        public async Task<ProfileResponseDto> Get([FromRoute] Guid id)
        {
            Profile profile = await _profileService.Get(id);
            return profile.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ProfileRequestDto dto)
        {
            Profile profile = await _profileService.Create(UserId, dto);
            return CreatedAtAction(nameof(Me), profile.ToDto());
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] ProfileRequestDto dto)
        {
            Profile profile = await _profileService.GetByUserId(UserId);
            await _profileService.Update(profile, dto);

            return CreatedAtAction(nameof(Me), profile.ToDto());
        }
    }
}
