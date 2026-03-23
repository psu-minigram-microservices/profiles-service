namespace Minigram.Profile.Controllers.Dto
{
    using Minigram.Profile.ApplicationContext.Models;

    public class RelationResponseDto
    {
        public tStatus Status { get; set; }

        public ProfileResponseDto Profile { get; set; } = null!;
    }
}
