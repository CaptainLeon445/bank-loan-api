using BankApp.Application.DTOs;

namespace BankApp.Application.Common;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
    Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
    Task<string> InitiatePasswordResetAsync(RequestResetDto dto);
    Task<bool> ExecutePasswordResetAsync(ExecuteResetDto dto);
}