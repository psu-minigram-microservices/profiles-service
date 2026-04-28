namespace Minigram.Auth.DTO
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [PasswordStrength(
            MinimumLength = 8,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true)]
        public string Password { get; set; } = string.Empty;
    }
}