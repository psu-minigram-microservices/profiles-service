namespace Minigram.Profile.Controllers.Dto
{
    using System.ComponentModel.DataAnnotations;

    public class ProfileRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }
    }
}
