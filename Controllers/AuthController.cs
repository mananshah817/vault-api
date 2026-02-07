using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterModel model)
    {
        using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
        var hash = BCrypt.Net.BCrypt.HashPassword(model.Password);

        var sql = @"insert into users(full_name, email, password_hash)
                    values(@FullName, @Email, @PasswordHash)";

        await con.ExecuteAsync(sql, new { model.FullName, model.Email, PasswordHash = hash });
        return Ok("Registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));

        var user = await con.QueryFirstOrDefaultAsync<dynamic>(
            "select * from users where email=@Email", new { model.Email });

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, (string)user.password_hash))
            return Unauthorized("Invalid credentials");

        var token = JwtHelper.GenerateToken(user.id.ToString(), user.email, _config);
        return Ok(new { token });
    }
}
