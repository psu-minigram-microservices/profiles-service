namespace Minigram.Auth.Models
{
    using Minigram.Core.ApplicationContext.Models;

    public class RefreshSession : BaseModel
    {
        public Guid UserId { get; set; }

        public Guid RefreshToken { get; set; }

        public string Ip { get; set; } = string.Empty;

        public string UserAgent { get; set; } = string.Empty;

        public DateTime ExpiresIn { get; set; }

        public User User { get; set; } = null!;
    }
}