namespace Hyre.API.Dtos.Auth
{
    public record AuthResponseDto(string AccessToken, string RefreshToken ,string FirstName, string LastName, string Email);

    public record RefreshTokenDto(string RefreshToken);
}
