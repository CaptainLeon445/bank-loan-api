using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            var response = await _userService.RegisterAsync(dto);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // Thrown by UserService when the email is already registered.
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Thrown by the User/Wallet constructors for invalid names, email or password hash.
            return BadRequest(new { Message = ex.Message });
        }
        catch (DbUpdateException)
        {
            // Two simultaneous sign-ups with the same email: the unique index on Users.Email rejects the second.
            return Conflict(new { Message = "User with this email already exists." });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto dto)
    {
        try
        {
            var response = await _userService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] RequestResetDto dto)
    {
        var securityToken = await _userService.InitiatePasswordResetAsync(dto);
        if (securityToken != null)
        {
            // Stand-in for sending an email until a mail provider is wired up.
            System.Console.WriteLine($"Password reset token for {dto.Email}: {securityToken}");
        }
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
