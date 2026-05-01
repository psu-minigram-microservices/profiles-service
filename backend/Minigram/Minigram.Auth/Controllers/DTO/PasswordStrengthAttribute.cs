namespace Minigram.Auth.DTO
{
    using System.ComponentModel.DataAnnotations;

    public class PasswordStrengthAttribute : ValidationAttribute
    {
        public int MinimumLength { get; set; } = 8;
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;

        public PasswordStrengthAttribute()
        {
            ErrorMessage = "Password does not meet the required strength criteria.";
        }

        public override bool IsValid(object? value)
        {
            if (value is not string password)
            {
                return false;
            }

            if (password.Length < MinimumLength)
            {
                ErrorMessage = $"Password must be at least {MinimumLength} characters long.";
                return false;
            }

            if (RequireDigit && !password.Any(char.IsDigit))
            {
                ErrorMessage = "Password must contain at least one digit.";
                return false;
            }

            if (RequireLowercase && !password.Any(char.IsLower))
            {
                ErrorMessage = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (RequireUppercase && !password.Any(char.IsUpper))
            {
                ErrorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (RequireNonAlphanumeric && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ErrorMessage = "Password must contain at least one non-alphanumeric character.";
                return false;
            }

            return true;
        }
    }
}