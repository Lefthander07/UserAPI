using UserAPI.Models.Core;
using System.ComponentModel.DataAnnotations;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для обновления профиля пользователя.
/// Используется при редактировании имени, пола или даты рождения.
/// </summary>
public record UpdateProfileDto
{
    /// <summary>
    /// Новое имя пользователя.
    /// Допустимы только латинские и русские буквы, пробелы.
    /// Если не указано — значение не изменяется.
    /// </summary>
    [RegularExpression("^[A-Za-z\u0400-\u04FF ]+$", ErrorMessage = "Name must contain only letters (Latin or Cyrillic).")]
    public string? Name { get; set; }

    /// <summary>
    /// Новый пол пользователя: 0 — женщина, 1 — мужчина, 2 — неизвестно.
    /// Если не указан — значение не изменяется.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Новая дата рождения пользователя.
    /// Если не указана — значение не изменяется.
    /// </summary>
    public DateTime? Birthday { get; set; }
}