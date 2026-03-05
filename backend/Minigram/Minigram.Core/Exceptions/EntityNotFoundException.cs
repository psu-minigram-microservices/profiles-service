namespace Minigram.Core.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException()
            : base("The entity was not found.") {}

        public EntityNotFoundException(string message)
            : base(message) {}
        
        public EntityNotFoundException(Type entityType)
            : base($"{entityType.Name} was not found.") {}

        public EntityNotFoundException(string entityName, object entityId)
            : base($"{entityName} with Id '{entityId}' was not found.") {}

        public EntityNotFoundException(Type entityType, object entityId)
            : base($"{entityType.Name} with Id '{entityId}' was not found.") {}

        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException) {}

        public EntityNotFoundException(string entityName, string fieldName, object fieldValue)
            : base($"{entityName} with {fieldName} '{fieldValue}' was not found.") {}
    }
}