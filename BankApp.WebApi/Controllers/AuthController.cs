using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using BankApp.Application.Users;
using BankApp.Application.Common;
using BankApp.Application.DTOs;



namespace BankApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto dto)
    {
        var response = await _userService.RegisterAsync(dto);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto dto)
    {
        var response = await _userService.LoginAsync(dto);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] RequestResetDto dto)
    {
        var securityToken = await _userService.InitiatePasswordResetAsync(dto);
        System.Console.WriteLine($"Password reset token for {dto.Email}: {securityToken}");
        return Ok(new { Message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ExecuteResetDto dto)
    {
        var success = await _userService.ExecutePasswordResetAsync(dto);
        if (!success) return BadRequest(new { Message = "Invalid or expired token." });
        return Ok(new { Message = "Password has been reset successfully." });
    }
}