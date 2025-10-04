namespace Hyre.API.Dtos.Auth
{
    public record RegisterDto(string FirstName, string LastName, string Email, string Password, string Phone);
}
