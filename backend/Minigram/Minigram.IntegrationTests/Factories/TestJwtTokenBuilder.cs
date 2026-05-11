using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Minigram.Profile.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TestJwtTokenBuilder
{
    private readonly SymmetricSecurityKey _securityKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly List<Claim> _claims = new();
    private DateTime _expires = DateTime.UtcNow.AddMinutes(15);

    public TestJwtTokenBuilder(JwtOptions jwtOptions)
    {
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        _issuer = jwtOptions.Issuer;
        _audience = jwtOptions.Audience;
    }

    public TestJwtTokenBuilder WithUserId(string userId)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId));
        return this;
    }

    public TestJwtTokenBuilder WithEmail(string email)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        return this;
    }

    public string Build()
    {
        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: _claims,
            expires: _expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}