using DocumentFormat.OpenXml.InkML;
using Hyre.API.Data;
using Hyre.API.Dtos.Auth;
using Hyre.API.Interfaces.Auth;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Hyre.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        /*public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                throw new Exception("Invalid credentials");

            

            var token = GenerateToken(user);
            return new AuthResponseDto(token, user.FirstName, user.LastName, user.Email);

        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.Phone
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            // Default role
            var roleResult = await _userManager.AddToRoleAsync(user, "Candidate");
            if (!roleResult.Succeeded)
                throw new Exception(string.Join("; ", roleResult.Errors.Select(e => e.Description)));


            var token = GenerateToken(user);
            return new AuthResponseDto(token, user.FirstName, user.LastName, user.Email);
        }*/
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid credentials");

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
                throw new Exception("Invalid credentials");

            var accessToken = GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            await SaveRefreshToken(user.Id, refreshToken);

            return new AuthResponseDto(
                accessToken,
                refreshToken,
                user.FirstName,
                user.LastName,
                user.Email
            );
        }

        // ---------------- REGISTER ----------------
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.Phone
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ",
                    result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Candidate");

            var accessToken = GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            await SaveRefreshToken(user.Id, refreshToken);

            return new AuthResponseDto(
                accessToken,
                refreshToken,
                user.FirstName,
                user.LastName,
                user.Email
            );
        }
        public async Task LogoutAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked);

            if (stored == null || stored.Expiry < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user == null)
                throw new Exception("User not found");

            // Revoke the old refresh token
            stored.IsRevoked = true;
            
            // Generate new tokens
            var newAccessToken = GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            
            // Save new refresh token
            await SaveRefreshToken(user.Id, newRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDto(
                newAccessToken,
                newRefreshToken,
                user.FirstName,
                user.LastName,
                user.Email
            );
        }

        private string GenerateToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = _userManager.GetRolesAsync(user).Result ?? new List<string>();
            claims.AddRange(roles.Select(role => new Claim("roles", role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<String>("AppSettings:Token")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<String>("AppSettings:Issuer"),
                audience: _configuration.GetValue<String>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        // ---------------- HELPERS ----------------

        private async Task SaveRefreshToken(string userId, string token)
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && (x.Expiry < DateTime.UtcNow || x.IsRevoked))
                .ToListAsync();
            
            _context.RefreshTokens.RemoveRange(expiredTokens);

            var rt = new RefreshToken
            {
                UserId = userId,
                Token = token,
                Expiry = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(rt);
            await _context.SaveChangesAsync();
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );
        }

       


    }
}
