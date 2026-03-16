    namespace Minigram.Profile.Controllers
    {
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.JsonPatch;
        using Minigram.Profile.Dto;
        using Minigram.Profile.Extensions;
        using Minigram.Profile.Services;
        using Minigram.Profile.Models;
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

            [HttpGet($"{{{nameof(userId)}}}")]
            public async Task<ProfileResponseDto> Get([FromRoute] Guid userId)
            {
                Profile profile = await _profileService.GetByUserId(userId);
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

            [HttpPatch]
            public async Task<ActionResult> Patch([FromBody] JsonPatchDocument<ProfileRequestDto> patch)
            {
                Profile profile = await _profileService.GetByUserId(UserId);
                await _profileService.Patch(profile, patch);

                return CreatedAtAction(nameof(Me), profile.ToDto());
            }
        }
    }
