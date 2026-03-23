namespace Minigram.Auth.Models
{
    using Minigram.Core.ApplicationContext.Models;

    public class User : BaseModel
    {
        public string Email { get; set; } = string.Empty;

        public bool IsEmailVerified { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}
