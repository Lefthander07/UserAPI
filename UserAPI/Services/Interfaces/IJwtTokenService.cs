using UserAPI.Models.Core;
namespace UserAPI.Services.interfaces;

public interface IJwtTokenService
{ 
    string GenerateToken(User user);
}
