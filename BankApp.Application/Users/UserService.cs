using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BankApp.Domain.Entities;
using BankApp.Application.Common;
using BankApp.Application.DTOs;

namespace BankApp.Application.Users;



public class UserService : IUserService
{
    private readonly IBankDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(IBankDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }


    public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
    {
        bool exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists) throw new InvalidOperationException("User with this email already exists.");

        string passwordHash = _passwordHasher.HashPassword(dto.Password);
        var user = new User(dto.FirstName, dto.LastName, dto.Email, passwordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        string token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResponseDto(token, user.Id, user.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        { throw new UnauthorizedAccessException("Invalid email or password."); }

        string token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResponseDto(token, user.Id, user.Email);
    }

    public async Task<string> InitiatePasswordResetAsync(RequestResetDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return "If the email exists, a password reset link has been sent.";

        string rawToken =  Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        user.GenerateResetToken(rawToken, TimeSpan.FromMinutes(15));
        await _context.SaveChangesAsync();
        return rawToken;
    }

    public async Task<bool> ExecutePasswordResetAsync(ExecuteResetDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);
        if (user == null || user.PasswordResetToken != dto.Token || user.ResetTokenExpiryUtc < DateTime.UtcNow)
        { return false; }

        string newPasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.UpdatePassword(newPasswordHash);
        await _context.SaveChangesAsync();
        return true;
    }

}