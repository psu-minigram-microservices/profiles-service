namespace Minigram.Profile.Models
{
    using NpgsqlTypes;

    public enum tRelationshipStatus
    {
        [PgName("Pending")]
        Pending,
        
        [PgName("Accepted")]
        Accepted,
        
        [PgName("Rejected")]
        Rejected,
        
        [PgName("Blocked")]
        Blocked
    }
}
