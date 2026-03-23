namespace Minigram.Profile.Extensions
{
    using Minigram.Profile.Controllers.Dto;
    using Minigram.Profile.ApplicationContext.Models;

    internal static class RelationExtensions
    {
        public static RelationResponseDto ToDto(this Relation relation)
        {
            ArgumentNullException.ThrowIfNull(relation);

            return new RelationResponseDto
            {
                Status = relation.Status,
                Profile = relation.Receiver.ToDto(),
            };
        }
    }
}