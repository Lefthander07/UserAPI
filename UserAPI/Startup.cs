using Audit.Http;
using Audit.Core;
using Microsoft.OpenApi.Models;
using Serilog;
using UserAPI.Middlewares;
using UserAPI.Models.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using UserAPI.Data;
using UserAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserAPI.Services.interfaces;
using UserAPI.Data.Interfaces;

namespace UserAPI;
public class Startup
{
	private readonly IConfiguration _configuration;

	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
        services.AddControllers();

        services.AddOptions<AuthOptions>()
            .Bind(_configuration.GetSection("JwtSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = _configuration.GetSection("JwtSettings").Get<AuthOptions>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = jwtSettings.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
            }; 
        });
        services.AddAuthorization();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        Configuration.Setup()
            .UseSerilog(
            config => config.Message(
            auditEvent =>
            {
                if (auditEvent is AuditEventHttpClient httpClientEvent)
                {
                    var contentBody = httpClientEvent.Action?.Response?.Content?.Body;
                    if (contentBody is string { Length: > 1000 } stringBody)
                    {
                        httpClientEvent.Action.Response.Content.Body = stringBody[..1000] + "<...>";
                    }
                }
                return auditEvent.ToJson();
            }));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UsersAPI",
                Version = "v1",
                Description = "API для управления пользователями"
            });

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT авторизация. Введите **Bearer &lt;токен&gt;** в поле ниже.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
        });

        services.AddDbContext<UsersDbContext>(
        optionsBuilder =>
        {
            optionsBuilder
            .UseNpgsql(
                connectionString: _configuration.GetConnectionString("Users"),
                npgsqlOptionsAction: sqlOptionBuilder =>
                {
                    sqlOptionBuilder.EnableRetryOnFailure();
                    sqlOptionBuilder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, UsersDbContext.SchemaName);
                })
            .UseSnakeCaseNamingConvention();
            optionsBuilder.EnableSensitiveDataLogging().LogTo(Console.WriteLine);
        });

        services.AddScoped<IUsersRepository, UsersRepository>();
    }

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Users API v1");
                options.RoutePrefix = "";
            });
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}