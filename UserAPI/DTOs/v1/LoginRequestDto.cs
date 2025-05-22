using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для запроса аутентификации пользователя.
/// Используется при входе в систему (получении JWT-токена).
/// </summary>
public record LoginRequestDto
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    [Required]
    [JsonPropertyName("login")]
    public string Login { get; init; } = string.Empty;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    [Required]
    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;
}

