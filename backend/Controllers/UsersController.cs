using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BE_DA_WEB.Models;
using BE_DA_WEB.Data;
using BCrypt.Net;
[ApiController]
[Route("api/[controller]")]
public class usersController : ControllerBase
{

    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    public usersController(ITokenService tokenService, IUserService userService)
    {
     
        _tokenService = tokenService;
        _userService = userService;
    }

     [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (isSuccess, message, user) = await _userService.RegisterAsync(dto);

        if (!isSuccess)
            return BadRequest(new { message });

        var accessToken = _tokenService.GenerateAccessToken(user!);
        var refreshToken = _tokenService.GenerateRefreshToken(user!);

        SetRefreshTokenCookie(refreshToken);

        return Ok(new
        {
            message,
            accessToken,
            user = new
            {
                user!.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.IsAdmin
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (isSuccess, message, user) = await _userService.LoginAsync(dto);

        if (!isSuccess)
            return BadRequest(new { message });

        var accessToken = _tokenService.GenerateAccessToken(user!);
        var refreshToken = _tokenService.GenerateRefreshToken(user!);

        SetRefreshTokenCookie(refreshToken);

        return Ok(new
        {
            message,
            accessToken,
            user = new
            {
                user!.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.IsAdmin
            }
        });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}
