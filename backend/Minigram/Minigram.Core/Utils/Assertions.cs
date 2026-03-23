using System.Diagnostics.CodeAnalysis;

namespace Minigram.Core.Utils
{
    public static class Assertions
    {
        public static void ThrowIfNullOrEmpty([NotNull] Guid? value, string? paramName = null)
        {
            string message = $"{(string.IsNullOrWhiteSpace(paramName) ? nameof(Guid) : paramName)} cannot be null or empty";

            if (value is null || value == Guid.Empty)
            {
                ThrowArgumentException(paramName, message);
            }
        }

        [DoesNotReturn]
        private static void ThrowArgumentException(string? paramName, string message) => throw new ArgumentException(message, paramName);
    }
}