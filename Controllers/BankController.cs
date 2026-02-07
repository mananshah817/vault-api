using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BankController : ControllerBase
{
    private readonly IConfiguration _config;

    public BankController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
public async Task<IActionResult> Get()
{
    try
    {
        using var con = new NpgsqlConnection(_connStr);
        var data = await con.QueryAsync("select id, bank_name, ifsc, branch from banks where user_id = @uid",
            new { uid = User.GetUserId() });

        return Ok(data);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);   // 🔥 see real error in browser
    }
}

[HttpPost]
public async Task<IActionResult> Add(BankModel model)
{
    try
    {
        using var con = new NpgsqlConnection(_connStr);

        await con.ExecuteAsync(@"
            insert into banks (bank_name, account_number, ifsc, branch, user_id)
            values (@BankName, @AccountNumber, @IFSC, @Branch, @UserId)",
            new
            {
                model.BankName,
                model.AccountNumber,
                model.IFSC,
                model.Branch,
                UserId = User.GetUserId()
            });

        return Ok(new { success = true });
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);   // 🔥 real error
    }
}
}
