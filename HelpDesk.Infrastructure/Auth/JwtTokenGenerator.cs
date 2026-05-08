using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDesk.Application.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(ActorContext actor)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, actor.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, actor.Email),
            new("role", actor.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
