using Library_Book_Borrowing_System.Dtos;

namespace Library_Book_Borrowing_System.Services;
public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}