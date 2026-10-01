using System.Threading.Tasks;
using TrustRent.Application.Auth.DTOs;

namespace TrustRent.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
