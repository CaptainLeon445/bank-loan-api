namespace BankApp.Application.DTOs;

public record AuthResponseDto(
    string Token, Guid UserId, string Email 
);