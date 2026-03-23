namespace Minigram.Profile.Extensions
{
    using Minigram.Profile.Controllers.Dto;
    using Minigram.Profile.ApplicationContext.Models;

    internal static class ProfileExtensions
    {
        public static ProfileResponseDto ToDto(this Profile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            return new ProfileResponseDto
            {
                Id = profile.Id,
                Name = profile.Name,
                PhotoUrl = profile.PhotoUrl,
            };
        }
    }
}