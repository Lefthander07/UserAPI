using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAPI.Data.Interfaces;
using UserAPI.DTOs.v1;

namespace UserAPI.Controllers.v1;

[ApiController]
[Route("v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUsersRepository _repo;
    public UsersController(IUsersRepository repo)
    {
        _repo = repo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole("Admin");

    /// <summary>
    /// Создать пользователя.
    /// </summary>
    /// <param name="dto">
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// Данные нового пользователя: логин, пароль, имя, пол, дата рождения, флаг «Администратор».
    /// </param>
    /// <returns>Созданный пользователь.</returns>
    /// <response code="201">Пользователь успешно создан.</response>
    /// <response code="400">Невалидные данные запроса.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав (роль Admin отсутствует).</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _repo.CreateAsync(
            dto.Login,
            dto.Password,
            dto.Name,
            dto.Gender,
            dto.Birthday,
            dto.Admin,
            createdBy: CurrentUserId,
            cancellationToken
        );

        var result = UserDto.FromModel(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
    }

    /// <summary>
    /// Получить список всех активных пользователей, отсортированный по <c>CreatedOn</c>.
    /// </summary>
    /// <returns>Коллекция активных пользователей.</returns>
    /// <response code="200">Список успешно получен.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetActive(CancellationToken cancellationToken)
    {
        var users = await _repo.GetActiveOrderedAsync(cancellationToken);
        return Ok(UserDto.FromModels(users));
    }

    /// <summary>
    /// Получить краткую информацию о пользователе по логину.
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>Имя, пол, дата рождения и признак активности.</returns>
    /// <response code="200">Пользователь найден.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpGet("by-login/{login}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserSummaryDto>> GetByLogin([FromRoute] string login, CancellationToken cancellationToken)
    {
        var user = await _repo.GetByLoginAsync(login, cancellationToken);
        if (user == null)
            return NotFound();

        var dto = new UserSummaryDto
        {
            Login = user.Login,
            Name = user.Name,
            Gender = user.Gender,
            Birthday = user.Birthday,
            IsActive = user.RevokedOn == null
        };
        return Ok(dto);
    }


    /// <summary>
    /// Аутентификация пользователя по логину и паролю.
    /// </summary>
    /// <param name="dto">Логин и пароль.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>Данные пользователя при успешной аутентификации.</returns>
    /// <response code="200">Успешная аутентификация.</response>
    /// <response code="400">Невалидные данные запроса.</response>
    /// <response code="401">Неверные учётные данные или пользователь деактивирован.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost("authenticate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> Authenticate([FromBody] AuthRequestDto dto, CancellationToken cancellationToken)
    {
        var user = await _repo.GetByCredentialsAsync(dto.Login, dto.Password, cancellationToken);
        if (user == null)
            return Unauthorized("Invalid credentials or user is deactivated.");

        var result = UserDto.FromModel(user);
        return Ok(result);
    }

    /// <summary>
    /// Получить всех пользователей старше указанного возраста.
    /// </summary>
    /// <param name="age">Возраст в годах.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>Коллекция пользователей.</returns>
    /// <response code="200">Список успешно получен.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpGet("older-than/{age:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetOlderThan([FromRoute] int age, CancellationToken cancellationToken)
    {
        var users = await _repo.GetOlderThanAsync(age, cancellationToken);
        return Ok(UserDto.FromModels(users));
    }

    /// <summary>
    /// Изменить профиль (имя, пол, дата рождения).
    /// </summary>
    /// <param name="id">ID изменяемого пользователя.</param>
    /// <param name="dto">Новые значения полей профиля.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <response code="204">Профиль успешно изменён.</response>
    /// <response code="400">Невалидные данные запроса.</response>
    /// <response code="403">Недостаточно прав или пользователь деактивирован.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPut("{id:guid}/profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!IsAdmin && id != CurrentUserId)
            return Forbid();

        if (!IsAdmin)
        {
            var self = await _repo.GetByIdAsync(id, cancellationToken);
            if (self?.RevokedOn != null)
                return Forbid("User is deactivated.");
        }

        var ok = await _repo.UpdateProfileAsync(
            id, dto.Name, dto.Gender, dto.Birthday, modifiedBy: CurrentUserId, cancellationToken
        );
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Изменить пароль.
    /// </summary>
    /// <param name="id">ID пользователя.</param>
    /// <param name="dto">Новый пароль.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <response code="204">Пароль успешно изменён.</response>
    /// <response code="400">Невалидные данные запроса.</response>
    /// <response code="403">Недостаточно прав или пользователь деактивирован.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!IsAdmin && id != CurrentUserId)
            return Forbid();

        if (!IsAdmin)
        {
            var self = await _repo.GetByIdAsync(id, cancellationToken);
            if (self?.RevokedOn != null)
                return Forbid("User is deactivated.");
        }

        var ok = await _repo.ChangePasswordAsync(id, dto.NewPassword, modifiedBy: CurrentUserId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Изменить логин.
    /// </summary>
    /// <param name="id">ID пользователя.</param>
    /// <param name="dto">Новый логин.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <response code="204">Логин успешно изменён.</response>
    /// <response code="400">Логин уже занят или пользователь не найден.</response>
    /// <response code="403">Недостаточно прав или пользователь деактивирован.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPut("{id:guid}/login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeLogin(Guid id, [FromBody] ChangeLoginDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!IsAdmin && id != CurrentUserId)
            return Forbid();

        if (!IsAdmin)
        {
            var self = await _repo.GetByIdAsync(id, cancellationToken);
            if (self?.RevokedOn != null)
                return Forbid("User is deactivated.");
        }

        var ok = await _repo.ChangeLoginAsync(id, dto.NewLogin, modifiedBy: CurrentUserId, cancellationToken);
        if (!ok)
            return BadRequest($"Login '{dto.NewLogin}' is already taken or user not found.");
        return NoContent();
    }


    /// <summary>
    /// Мягкое удаление пользователя по логину (ставит <c>RevokedOn</c>/<c>RevokedBy</c>).
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <response code="204">Пользователь деактивирован.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpDelete("{login}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SoftDelete([FromRoute] string login, CancellationToken cancellationToken)
    {
        var ok = await _repo.SoftDeleteByLoginAsync(login, revokedBy: CurrentUserId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Полное удаление пользователя по логину (безвозвратно).
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <response code="204">Пользователь удалён.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpDelete("{login}/permanent")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HardDelete([FromRoute] string login, CancellationToken cancellationToken)
    {
        var ok = await _repo.HardDeleteByLoginAsync(login, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Восстановить пользователя после мягкого удаления.
    /// </summary>
    /// <param name="id">ID пользователя.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <response code="204">Пользователь восстановлен.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _repo.RestoreAsync(id, restoredBy: CurrentUserId, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Получить пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции.</param>
    /// <returns>Полные данные пользователя.</returns>
    /// <response code="200">Пользователь найден.</response>
    /// <response code="403">Недостаточно прав или пользователь деактивирован.</response>
    /// <response code="404">Пользователь не найден.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound();
        if (!IsAdmin && id != CurrentUserId)
            return Forbid();
        if (!IsAdmin && user.RevokedOn != null)
            return Forbid("User is deactivated.");

        return Ok(UserDto.FromModel(user));
    }
}
