using System;
namespace BankApp.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? ResetTokenExpiryUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }


    public User(string firstName, string lastName, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        FirstName = string.IsNullOrWhiteSpace(firstName) ? throw new ArgumentException("Invalid first name.") : firstName;
        LastName = string.IsNullOrWhiteSpace(lastName) ? throw new ArgumentException("Invalid last name.") : lastName;
        Email = string.IsNullOrWhiteSpace(email) || !email.Contains('@') ? throw new ArgumentException("Invalid email.") : email;
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? throw new ArgumentException("Invalid password") : passwordHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        ClearResetToken();
    }

    public void GenerateResetToken(string token, TimeSpan validityDuration)
    {
        PasswordResetToken= token;
        ResetTokenExpiryUtc= DateTime.UtcNow.Add(validityDuration);
    }

    public void ClearResetToken()
    {
        PasswordResetToken= null;
        ResetTokenExpiryUtc= null;
    }
}