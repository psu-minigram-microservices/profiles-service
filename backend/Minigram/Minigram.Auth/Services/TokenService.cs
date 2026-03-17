namespace Minigram.Auth.Services
{
    using System.Text;
    using System.Security.Claims;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Minigram.Auth.Models;
    using Minigram.Auth.Options;

    public class TokenService
    {
        private readonly JwtOptions _jwtOptions;

        public TokenService(IOptions<JwtOptions> jwtOption)
        {
            _jwtOptions = jwtOption.Value;
        }

        public async Task<string> Generate(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            List<Claim> claims = new ()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            SymmetricSecurityKey key = new (Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            SigningCredentials credentials = new (key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new (
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Expiration),
                signingCredentials: credentials);

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}