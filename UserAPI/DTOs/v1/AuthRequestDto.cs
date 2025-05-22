using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для запроса аутентификации пользователя.
/// </summary>
public class AuthRequestDto
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    [Required]
    [JsonPropertyName("login")]
    public string Login { get; set; } = default!;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; } = default!;
}

