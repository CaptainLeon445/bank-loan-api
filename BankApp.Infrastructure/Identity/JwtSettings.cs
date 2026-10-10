using System;
using System.Text;

namespace BankApp.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;

    public byte[] KeyBytes => Encoding.UTF8.GetBytes(Key);

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new InvalidOperationException("Jwt:Key is not configured. Set it with user-secrets (development) or the Jwt__Key environment variable.");
        if (KeyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HMAC-SHA256.");
        if (string.IsNullOrWhiteSpace(Issuer)) throw new InvalidOperationException("Jwt:Issuer is not configured.");
        if (string.IsNullOrWhiteSpace(Audience)) throw new InvalidOperationException("Jwt:Audience is not configured.");
        if (ExpiryMinutes <= 0) throw new InvalidOperationException("Jwt:ExpiryMinutes must be greater than zero.");
    }
}
