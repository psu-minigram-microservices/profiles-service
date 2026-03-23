namespace Minigram.Profile.ApplicationContext.Models
{
    using Minigram.Core.ApplicationContext.Models;

    public class Relation : BaseModel
    {
        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }

        public tStatus Status { get; set; }

        public virtual Profile Sender { get; set; } = null!;

        public virtual Profile Receiver { get; set; } = null!;
    }
}
