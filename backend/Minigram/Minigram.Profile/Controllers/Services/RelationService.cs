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


    public class RelationService
    {
        private readonly IRepository<Relation> _relationRepository;

        private IQueryable<Relation> Relations => _relationRepository.Get();

        public RelationService(IRepository<Relation> relationRepository)
        {
            _relationRepository = relationRepository;
        }

        public async Task<List<ProfileResponseDto>> GetAllByStatus(Guid senderId, tStatus status, QueryParams queryParams)
        {
            ArgumentNullException.ThrowIfNull(queryParams);
            Assertions.ThrowIfNullOrEmpty(senderId, nameof(senderId));

            IQueryable<Relation> relations = Relations;

            int? page = queryParams.Page;
            int? perPage = queryParams.PerPage;

            if (page.HasValue && perPage.HasValue)
            {
                relations.Skip(page.Value * perPage.Value).Take(perPage.Value);
            }

            return await relations
                .Where(r => r.Status == status && r.SenderId == senderId)
                .Select(u => u.Receiver.ToDto())
                .ToListAsync();
        }

        public async Task<int> CountByStatus(Guid senderId, tStatus status)
        {
            Assertions.ThrowIfNullOrEmpty(senderId, nameof(senderId));

            return await Relations
                .Where(r => r.Status == status && r.SenderId == senderId)
                .CountAsync();
        }

        public async Task<RelationResponseDto> Get(Guid senderId, Guid receiverId)
        {
            Assertions.ThrowIfNullOrEmpty(senderId, nameof(senderId));
            Assertions.ThrowIfNullOrEmpty(receiverId, nameof(receiverId));

            RelationResponseDto? relation = await Relations
                .Where(r => r.SenderId == senderId && r.ReceiverId == receiverId)
                .Select(r => r.ToDto())
                .FirstOrDefaultAsync();

            if (relation == null)
            {
                throw new EntityNotFoundException(typeof(Relation));
            }
            
            return relation;
        }

        public async Task<Relation> Send(Guid senderId, Guid receiverId)
        {
            Assertions.ThrowIfNullOrEmpty(senderId, nameof(senderId));
            Assertions.ThrowIfNullOrEmpty(receiverId, nameof(receiverId));

            Relation relation = new ()
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = tStatus.None,
            };

            await _relationRepository.Create(relation);
            await _relationRepository.SaveAsync();

            return relation;
        }

        public async Task Reply(Guid senderId, Guid receiverId, tReplyStatus status)
        {
            Assertions.ThrowIfNullOrEmpty(senderId, nameof(senderId));
            Assertions.ThrowIfNullOrEmpty(receiverId, nameof(receiverId));

            Relation? relation  = await Relations
                .Where(r => r.SenderId == senderId && r.ReceiverId == receiverId)
                .FirstOrDefaultAsync();

            if (relation is null)
            {
                throw new EntityNotFoundException(typeof(Relation));
            }

            if (relation.Status != tStatus.None)
            {
                throw new InvalidOperationException($"Cannot reply to relation with status {relation.Status}.");
            }

            if (status == tReplyStatus.Accepted)
            {
                relation.Status = tStatus.Friend;

                if (await Relations.AnyAsync(r => r.SenderId == receiverId && r.ReceiverId == senderId) == false)
                {
                    Relation reverseRelation = new ()
                    {
                        Id = Guid.NewGuid(),
                        SenderId = receiverId,
                        ReceiverId = senderId,
                        Status = tStatus.Friend
                    };

                    await _relationRepository.Create(reverseRelation);
                }
            }
            else if (status == tReplyStatus.Blocked)
            {
                relation.Status = tStatus.Blocked;
            }
            else if (status == tReplyStatus.Rejected)
            {
                _relationRepository.Delete(relation);
            }

            await _relationRepository.SaveAsync();
        }

        public async Task Delete(Guid senderId, Guid receiverId)
        {
            Assertions.ThrowIfNullOrEmpty(senderId, nameof(senderId));
            Assertions.ThrowIfNullOrEmpty(receiverId, nameof(receiverId));

            Relation? relation = await Relations.FirstOrDefaultAsync(
                r => r.SenderId == senderId && r.ReceiverId == receiverId);

            if (relation == null)
            {
                throw new EntityNotFoundException(typeof(Relation));
            }

            _relationRepository.Delete(relation);
            await _relationRepository.SaveAsync();
        }
    }
}