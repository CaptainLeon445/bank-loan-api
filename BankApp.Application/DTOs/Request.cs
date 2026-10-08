using System;
using System.ComponentModel.DataAnnotations;


namespace BankApp.Application.DTOs;

public record RegisterUserDto(
    [Required][MaxLength(50)] string FirstName, [Required][MaxLength(50)] string LastName,
    [Required][EmailAddress] string Email, [Required][MinLength(6)] string Password
);

public record LoginUserDto(
    [Required][EmailAddress] string Email, [Required][MinLength(6)] string Password
);

public record RequestResetDto(
    [Required][EmailAddress] string Email
);

public record ExecuteResetDto(
    [Required] string Token, [Required][MinLength(6)] string NewPassword
);
