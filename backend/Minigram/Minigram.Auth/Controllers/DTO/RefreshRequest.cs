namespace Minigram.Auth.DTO
{
    using System.ComponentModel.DataAnnotations;

    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
