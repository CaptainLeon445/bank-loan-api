using System;
using System.Security.Claims;

namespace BankApp.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    // JwtTokenGenerator puts the user id in the NameIdentifier claim.
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Token does not contain a valid user id.");
    }
}
