using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Library_Book_Borrowing_System.Services;
public class AuthService : IAuthService
{
    private readonly IMemberRepository _userRepository;
    private readonly IPasswordHasher<Member> _passwordHasher;
    private readonly JwtSettings _jwtSettings;
    public AuthService(
        IMemberRepository userRepository,
        IPasswordHasher<Member> passwordHasher,
        IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<(AuthResponse? Response, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.EmailExistsAsync(email)) 
            return (null, "A user with this email already exists.");

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            MembershipDate = DateTime.Now.ToString("MM/dd/yyyy")
        };

        member.PasswordHash = _passwordHasher.HashPassword(member, request.Password);
        _userRepository.Add(member);

        return (GenerateToken(member), null);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null) return null;

        var result = _passwordHasher.VerifyHashedPassword(
        user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed) return null;
        return GenerateToken(user);
    }

    private AuthResponse GenerateToken(Member member)
    {
        var expiresAtUtc = DateTime.UtcNow
        .AddMinutes(_jwtSettings.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, member.Id.ToString()),
            new(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new(ClaimTypes.Email, member.Email),
            new(ClaimTypes.GivenName, member.FullName),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            MemberId = member.Id,
            Email = member.Email,
        };
    }
}