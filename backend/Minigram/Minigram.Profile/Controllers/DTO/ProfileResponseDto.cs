namespace Minigram.Profile.Controllers.Dto
{
    public class ProfileResponseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}
