    namespace Minigram.Profile.Controllers
    {
        using Microsoft.AspNetCore.Mvc;
        using Minigram.Profile.Dto;
        using Minigram.Profile.Extensions;
        using Minigram.Profile.Services;
        using Minigram.Profile.Models;
        using Minigram.Core.Dto;

        [ApiVersion("1.0")]
        [ApiController]
        [Route($"{nameof(Profile)}s/[controller]s")]
        public class RelationController : ControllerBase
        {
            private readonly CurrentUserService _currentUserService;

            private readonly RelationService _relationService;

            private Guid UserId =>
                _currentUserService.UserGuid ?? throw new UnauthorizedAccessException();

            public RelationController(
                CurrentUserService currentUserService,
                RelationService relationService)
            {
                _currentUserService = currentUserService;
                _relationService = relationService;
            }

            [HttpGet]
            public async Task<PagedResponse<ProfileResponseDto>> GetByStatus(
                [FromQuery] tRelationshipStatus status,
                [FromQuery] QueryParams queryParams)
            {
                int count = await _relationService.CountByStatus(UserId, status);
                List<ProfileResponseDto> data = await _relationService.GetAllByStatus(UserId, status, queryParams);

                return new PagedResponse<ProfileResponseDto>
                {
                    Count = count,
                    Data = data,
                };
            }

            [HttpGet($"{{{nameof(receiverId)}}}")]
            public async Task<RelationResponseDto> Get([FromRoute] Guid receiverId)
            {
                return await _relationService.Get(UserId, receiverId);
            }

            [HttpPost($"{{{nameof(receiverId)}}}")]
            public async Task<ActionResult> CreateOrUpdate(
                [FromRoute] Guid receiverId,
                [FromQuery] tRelationshipStatus status)
            {
                Relation relation = await _relationService.CreateOrUpdate(UserId, receiverId, status);
                return CreatedAtAction(nameof(Get), relation.ToDto());
            }

            [HttpDelete($"{{{nameof(receiverId)}}}")]
            public async Task DeleteRelation([FromRoute] Guid receiverId)
            {
                await _relationService.Delete(UserId, receiverId);
            }
        }
    }
