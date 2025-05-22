using UserAPI.Models.Core;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO с краткой информацией о пользователе.
/// Используется, например, при запросе сведений по логину.
/// </summary>
public record UserSummaryDto
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; init; } = default!;

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Пол пользователя: 0 — женщина, 1 — мужчина, 2 — неизвестно.
    /// </summary>
    public Gender Gender { get; init; }

    /// <summary>
    /// Дата рождения пользователя (может быть не указана).
    /// </summary>
    public DateTime? Birthday { get; init; }

    /// <summary>
    /// Признак активности пользователя.
    /// True — пользователь активен (не удалён), False — деактивирован.
    /// </summary>
    public bool IsActive { get; init; }
}

