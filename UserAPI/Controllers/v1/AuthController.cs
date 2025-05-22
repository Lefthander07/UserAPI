namespace UserAPI.Controllers.v1;

using UserAPI.Data;
using UserAPI.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAPI.DTOs.v1;


/// <summary>
/// Аутентификация пользователя и выдача JWT-токена.
/// </summary>
[Route("v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsersDbContext _context;
    private readonly IJwtTokenService _tokenService;
    public AuthController(UsersDbContext context, IConfiguration config, IJwtTokenService tokenService)

    {
        _tokenService = tokenService;
        _context = context;
    }


    /// <summary>
    /// Аутентификация пользователя и выдача JWT-токена.
    /// </summary>
    /// <param name="loginDto">Данные для входа: логин и пароль.</param>
    /// <returns>JWT-токен в случае успешной аутентификации.</returns>
    /// <response code="200">Успешная аутентификация. Возвращает JWT-токен.</response>
    /// <response code="400">Ошибочные данные запроса (невалидная модель).</response>
    /// <response code="401">Неверный логин или пароль, либо пользователь деактивирован.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Login([FromBody] LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = _context.Users
            .SingleOrDefault(u => u.Login == loginDto.Login && u.RevokedOn == null);

        if (user == null)
        {
            return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized",
                    detail: "Неверный логин или пароль, либо пользователь деактивирован."
            );
        }

        if (user.Password != loginDto.Password)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Неверный логин или пароль, либо пользователь деактивирован."
            );
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new { token });
    }
}

