namespace Minigram.Profile.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;
    using Minigram.Profile.Controllers.Dto;
    using Minigram.Profile.Controllers.Services;
    using Minigram.Profile.ApplicationContext.Models;
    using Minigram.Core.Dto;


    [ApiVersion("1.0")]
    [ApiController]
    [Route($"{nameof(Profile)}s/[controller]s")]
    public class RelationController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;

        private readonly RelationService _relationService;

        private readonly ProfileService _profileService;

        private Guid UserId =>
            _currentUserService.UserGuid ?? throw new UnauthorizedAccessException();

        public RelationController(
            CurrentUserService currentUserService,
            RelationService relationService,
            ProfileService profileService)
        {
            _currentUserService = currentUserService;
            _relationService = relationService;
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<PagedResponse<ProfileResponseDto>> GetByStatus(
            [Required][FromQuery] tStatus status,
            [Required][FromQuery] tRelationType type,
            [FromQuery] QueryParams queryParams)
        {
            Profile profile = await _profileService.GetByUserId(UserId);

            int count = await _relationService.CountByStatus(profile.Id, type, status);
            List<ProfileResponseDto> data = await _relationService.GetAllByStatus(profile.Id, status, type, queryParams);

            return new PagedResponse<ProfileResponseDto>
            {
                Count = count,
                Data = data,
            };
        }

        [HttpGet($"{{{nameof(receiverId)}}}")]
        public async Task<RelationResponseDto> Get(
            [FromRoute] Guid receiverId,
            [Required][FromQuery] tRelationType type)
        {
            Profile profile = await _profileService.GetByUserId(UserId);

            if (type == tRelationType.Outgoing)
            {
                return await _relationService.Get(profile.Id, receiverId);
            }
            
            return await _relationService.Get(receiverId, profile.Id);
        }

        [HttpPost($"{nameof(Send)}/{{{nameof(receiverId)}}}")]
        public async Task Send([FromRoute] Guid receiverId)
        {
            Profile profile = await _profileService.GetByUserId(UserId);
            await _relationService.Send(profile.Id, receiverId);
        }

        [HttpPost($"{nameof(Reply)}/{{{nameof(senderId)}}}")]
        public async Task Reply([FromRoute] Guid senderId, [Required][FromQuery] tReplyStatus status)
        {
            Profile profile = await _profileService.GetByUserId(UserId);
            await _relationService.Reply(senderId, profile.Id, status);
        }

        [HttpDelete($"{{{nameof(receiverId)}}}")]
        public async Task DeleteRelation([FromRoute] Guid receiverId)
        {
            Profile profile = await _profileService.GetByUserId(UserId);
            await _relationService.Delete(profile.Id, receiverId);
        }
    }
}
