namespace Minigram.Auth.Dto.User
{
    public class ReadProfileDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}
