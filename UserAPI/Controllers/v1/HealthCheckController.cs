using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserAPI.Controllers.v1;

/// <summary>
/// Проверка работоспособности текущего API.
/// </summary>
[ApiController]
[Route("healthcheck")]
public class HealthCheckController : ControllerBase
{
    /// <summary>
    /// Возвращает <c>200 OK</c>, если сервис запущен и способен обрабатывать запросы.
    /// </summary>
    /// <response code="200">Сервис работает.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthCheckResult), StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResult> Check()
    {
        var result = new HealthCheckResult
        {
            CheckedOn = DateTimeOffset.UtcNow,
            Status = HealthCheckResult.CheckStatus.Ok
        };

        return Ok(result);
    }
}

/// <summary>Результат проверки.</summary>
public record HealthCheckResult
{
    /// <summary>Момент времени, когда выполнена проверка.</summary>
    public DateTimeOffset CheckedOn { get; init; }

    /// <summary>Состояние сервиса.</summary>
    public CheckStatus Status { get; init; }

    /// <summary>Возможные состояния.</summary>
    public enum CheckStatus
    {
        /// <summary>Сервис работает.</summary>
        Ok = 1,

        /// <summary>Сервис недоступен (в обычных условиях этот вариант не вернётся, 
        /// потому что при ошибке контроллер не выполнится).</summary>
        Failed = 2
    }
}