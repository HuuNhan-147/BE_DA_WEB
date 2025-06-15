using BE_DA_WEB.Data;
using BE_DA_WEB.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public interface IUserService
{
    Task<(bool IsSuccess, string Message, User? User)> RegisterAsync(RegisterDto dto);
    Task<(bool IsSuccess, string Message, User? User)> LoginAsync(LoginDto dto);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsSuccess, string Message, User? User)> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return (false, "Email đã tồn tại!", null);

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = hashedPassword,
            Phone = dto.Phone,
            IsAdmin = dto.IsAdmin
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "Tạo tài khoản thành công!", user);
    }

    public async Task<(bool IsSuccess, string Message, User? User)> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            return (false, "Email hoặc mật khẩu không đúng!", null);

        return (true, "Đăng nhập thành công!", user);
    }
}
