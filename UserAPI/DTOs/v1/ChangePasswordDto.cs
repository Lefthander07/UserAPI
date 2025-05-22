using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для изменения пароля пользователя.
/// Используется при смене пароля текущим пользователем или администратором.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Новый пароль пользователя.
    /// Допустимы только латинские буквы и цифры, без пробелов и спецсимволов.
    /// </summary>
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Password must contain only Latin letters and digits.")]
    [JsonPropertyName("newpassword")]
    public string NewPassword { get; set; } = default!;
}

