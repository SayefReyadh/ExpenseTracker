using ExpenseTrackerAPI.DTOs;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> Register(RegisterDto registerDto);
    Task<AuthResponseDto?> Login(LoginDto loginDto);
    Task<User?> GetUserById(Guid userId);
}
