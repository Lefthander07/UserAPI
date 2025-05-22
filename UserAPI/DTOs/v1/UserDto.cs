using UserAPI.Models.Core;

namespace UserAPI.DTOs.v1;

/// <summary>
/// DTO для отображения информации о пользователе.
/// Используется при возврате данных из API (например, список пользователей или детали одного пользователя).
/// </summary>
public record UserDto
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Уникальный логин пользователя.
    /// </summary>
    public string Login { get; set; } = default!;

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Пол пользователя: 0 — женщина, 1 — мужчина, 2 — неизвестно.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Дата рождения пользователя (может быть не указана).
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Признак, является ли пользователь администратором.
    /// </summary>
    public bool Admin { get; set; }

    /// <summary>
    /// Дата создания пользователя.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Идентификатор пользователя, который создал этого пользователя.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Дата последнего изменения пользователя (если было).
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Идентификатор пользователя, выполнившего последнее изменение (если было).
    /// </summary>
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// Дата удаления (деактивации) пользователя (если был удалён).
    /// </summary>
    public DateTime? RevokedOn { get; set; }

    /// <summary>
    /// Идентификатор пользователя, выполнившего удаление (если было).
    /// </summary>
    public Guid? RevokedBy { get; set; }

    /// <summary>
    /// Преобразует сущность <see cref="User"/> в DTO <see cref="UserDto"/>.
    /// </summary>
    public static UserDto FromModel(User u) => new()
    {
        Id = u.Id,
        Login = u.Login,
        Name = u.Name,
        Gender = u.Gender,
        Birthday = u.Birthday,
        Admin = u.Admin,
        CreatedOn = u.CreatedOn,
        CreatedBy = u.CreatedBy,
        ModifiedOn = u.ModifiedOn,
        ModifiedBy = u.ModifiedBy,
        RevokedOn = u.RevokedOn,
        RevokedBy = u.RevokedBy
    };

    /// <summary>
    /// Преобразует коллекцию сущностей <see cref="User"/> в список DTO <see cref="UserDto"/>.
    /// </summary>
    public static IEnumerable<UserDto> FromModels(IEnumerable<User> users)
        => users.Select(FromModel);
}

