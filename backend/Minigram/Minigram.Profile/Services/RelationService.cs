namespace Minigram.Profile.Services
{
    using Microsoft.EntityFrameworkCore;
    using Minigram.Core.Dto;
    using Minigram.Core.Exceptions;
    using Minigram.Core.Repositories;
    using Minigram.Profile.Dto;
    using Minigram.Profile.Models;
    using Minigram.Profile.Extensions;

    public class RelationService
    {
        private readonly IRepository<Relation> _relationRepository;

        private IQueryable<Relation> Relations => _relationRepository.Get();

        public RelationService(IRepository<Relation> relationRepository)
        {
            _relationRepository = relationRepository;
        }

        public async Task<List<ProfileResponseDto>> GetAllByStatus(
            Guid senderId,
            tRelationshipStatus status,
            QueryParams queryParams)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            if (senderId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(senderId)} cannot be {senderId}", nameof(senderId));
            }

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

        public async Task<int> CountByStatus(Guid senderId, tRelationshipStatus status)
        {
            if (senderId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(senderId)} cannot be {senderId}", nameof(senderId));
            }

            return await Relations
                .Where(r => r.Status == status && r.SenderId == senderId)
                .CountAsync();
        }

        public async Task<RelationResponseDto> Get(Guid senderId, Guid receiverId)
        {
            if (senderId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(senderId)} cannot be {senderId}", nameof(senderId));
            }

            if (receiverId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(receiverId)} cannot be {receiverId}", nameof(receiverId));
            }

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

        public async Task<Relation> CreateOrUpdate(Guid senderId, Guid receiverId, tRelationshipStatus status)
        {
            if (senderId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(senderId)} cannot be {senderId}", nameof(senderId));
            }

            if (receiverId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(receiverId)} cannot be {receiverId}", nameof(receiverId));
            }

            Relation? relation = await Relations.FirstOrDefaultAsync(
                r => r.SenderId == senderId && r.ReceiverId == receiverId);

            if (relation == null)
            {
                relation = new Relation
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderId,
                    ReceiverId = receiverId
                };

                await _relationRepository.Create(relation);
            }

            relation.Status = status;
            await _relationRepository.SaveAsync();

            return relation;
        }

        public async Task Delete(Guid senderId, Guid receiverId)
        {
            if (senderId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(senderId)} cannot be {senderId}", nameof(senderId));
            }

            if (receiverId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(receiverId)} cannot be {receiverId}", nameof(receiverId));
            }

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