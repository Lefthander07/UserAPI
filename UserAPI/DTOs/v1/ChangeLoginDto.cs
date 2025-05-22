using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для изменения логина пользователя.
/// Используется при запросе смены логина текущего пользователя или администратором.
/// </summary>
public class ChangeLoginDto
{
    /// <summary>
    /// Новый логин пользователя.
    /// Допустимы только латинские буквы и цифры, без пробелов и спецсимволов.
    /// </summary>
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Login must contain only Latin letters and digits.")]
    [JsonPropertyName("newlogin")]
    public string NewLogin { get; set; } = default!;
}

