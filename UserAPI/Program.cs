using UserAPI;
using UserAPI.Models.Core;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Filters;
using UserAPI.Data;
using Microsoft.EntityFrameworkCore;

var webHost = Host
	.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webHostBuilder => webHostBuilder.UseStartup<Startup>())
    .UseSerilog((context, _, LoggerConfiguration) => LoggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails(
                new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithFilter(
                    new IgnorePropertyByNameExceptionFilter(
                        nameof(Exception.StackTrace),
                        nameof(Exception.Message),
                        nameof(Exception.TargetSite),
                        nameof(Exception.Source),
                        nameof(Exception.HResult), "Type"))))
.Build();

using (var scope = webHost.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Admin))
    {
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Login = "admin",
            Password = "admin", 
            Name = "Administrator",
            Gender = Gender.Unknown,
            Birthday = null,
            Admin = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };
        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

await webHost.RunAsync();