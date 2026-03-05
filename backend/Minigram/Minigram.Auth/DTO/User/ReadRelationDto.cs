namespace Minigram.Auth.Dto.User
{
    using Minigram.Core.Models;

    public class ReadRelationDto
    {
        public tRelationshipStatus Status { get; set; }

        public ReadProfileDto Profile { get; set; } = null!;
    }
}
