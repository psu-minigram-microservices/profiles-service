namespace Minigram.Profile.Models
{
    using Minigram.Core.Models;

    public class Relation : BaseModel
    {
        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }

        public tRelationshipStatus Status { get; set; }

        public virtual Profile Sender { get; set; } = null!;

        public virtual Profile Receiver { get; set; } = null!;
    }
}
