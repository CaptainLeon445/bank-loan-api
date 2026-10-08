using System;
using BankApp.Application.Common;

namespace BankApp.Infrastructure.Identity;

public class PasswordHasher: IPasswordHasher
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password,11);
    public bool VerifyPassword(string password, string passwordHash)=> BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
}