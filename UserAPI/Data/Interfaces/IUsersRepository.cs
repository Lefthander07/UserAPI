namespace UserAPI.Data.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserAPI.Models.Core;

/// <summary>
/// Абстракция доступа к пользователям.
/// Не содержит деталей EF Core — только бизнес-операции.
/// </summary>
public interface IUsersRepository
{
    /* ---------- Create ---------- */

    /// <summary>
    /// Создать нового пользователя (п. 1). 
    /// </summary>
    Task<User> CreateAsync(
        string login,
        string password,
        string name,
        Gender gender,
        DateTime? birthday,
        bool isAdmin,
        Guid createdBy,
        CancellationToken cancellationToken);


    /// <summary>
    /// Вернуть всех активных пользователей, отсортированных по <see cref="User.CreatedOn"/> (п. 5).
    /// </summary>
    Task<IReadOnlyList<User>> GetActiveOrderedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Найти пользователя по логину.
    /// </summary>
    Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);

    /// <summary>
    /// Найти пользователя по логину и паролю.
    /// Возвращает <c>null</c>, если не найден или деактивирован.
    /// </summary>
    Task<User?> GetByCredentialsAsync(string login, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Вернуть всех пользователей, которые старше указанного возраста (п. 8).
    /// </summary>
    Task<IReadOnlyList<User>> GetOlderThanAsync(int ageYears, CancellationToken cancellationToken);

    /// <summary>
    /// Получить пользователя по <paramref name="id"/> (внутреннее использование контроллерами).
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Обновить имя, пол или дату рождения (п. 2). 
    /// </summary>
    Task<bool> UpdateProfileAsync(
        Guid userId,
        string? newName,
        Gender? newGender,
        DateTime? newBirthday,
        Guid modifiedBy,
        CancellationToken cancellationToken);

    /// <summary>
    /// Изменить пароль (п. 3).
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, string newPassword, Guid modifiedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Изменить логин (п. 4). Возвращает <c>false</c>, если логин уже занят.
    /// </summary>
    Task<bool> ChangeLoginAsync(Guid userId, string newLogin, Guid modifiedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Мягкое удаление по логину — прописывает <c>RevokedOn/RevokedBy</c> (п. 9).
    /// </summary>
    Task<bool> SoftDeleteByLoginAsync(string login, Guid revokedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Полное удаление по логину (п. 9).
    /// </summary>
    Task<bool> HardDeleteByLoginAsync(string login, CancellationToken cancellationToken);

    /// <summary>
    /// Восстановление пользователя — очищает <c>RevokedOn/RevokedBy</c> (п. 10).
    /// </summary>
    Task<bool> RestoreAsync(Guid userId, Guid restoredBy, CancellationToken cancellationToken);
}
