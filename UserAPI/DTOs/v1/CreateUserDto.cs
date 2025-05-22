using UserAPI.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для создания нового пользователя.
/// Используется администратором при добавлении нового пользователя в систему.
/// </summary>
public record CreateUserDto
{
    /// <summary>
    /// Логин пользователя (уникальный).
    /// Допустимы только латинские буквы и цифры, без пробелов и спецсимволов.
    /// </summary>
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Login must contain only Latin letters and digits.")]
    [JsonPropertyName("login")]
    public string Login { get; set; } = default!;

    /// <summary>
    /// Пароль пользователя.
    /// Допустимы только латинские буквы и цифры.
    /// </summary>
    [Required]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Password must contain only Latin letters and digits.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = default!;

    /// <summary>
    /// Имя пользователя.
    /// Допустимы только латинские и русские буквы, пробелы.
    /// </summary>
    [Required]
    [RegularExpression("^[A-Za-z\u0400-\u04FF ]+$", ErrorMessage = "Name must contain only letters (Latin or Cyrillic).")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Пол пользователя: 0 — женщина, 1 — мужчина, 2 — неизвестно.
    /// </summary>
    [Required]
    [JsonPropertyName("gender")]
    public Gender Gender { get; set; }

    /// <summary>
    /// Дата рождения пользователя (может быть не указана).
    /// </summary>
    [JsonPropertyName("birthday")]
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Является ли пользователь администратором.
    /// </summary>
    [JsonPropertyName("admin")]
    public bool Admin { get; set; }
}

