namespace UserAPI.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            _logger.LogInformation("Incoming Request: {Method} {Url}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            foreach (var param in httpContext.Request.Query)
            {
                _logger.LogInformation("Query Parameter: {Key} = {Value}", param.Key, param.Value);
            }

            await _next(httpContext);
            _logger.LogInformation("Response Status Code: {StatusCode}", httpContext.Response.StatusCode);
        }
    }
}
