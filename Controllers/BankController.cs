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

    [HttpPost]
    public async Task<IActionResult> Add(BankModel model)
    {
        var userId = User.FindFirst("id")?.Value;
        var encAcc = CryptoHelper.Encrypt(model.AccountNumber, _config["Crypto:Key"]);

        using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
        await con.ExecuteAsync(
            @"insert into bank_accounts(user_id, bank_name, account_enc, ifsc, branch)
              values(@UserId, @BankName, @AccountEnc, @IFSC, @Branch)",
            new { UserId = userId, model.BankName, AccountEnc = encAcc, model.IFSC, model.Branch });

        return Ok("Saved");
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = User.FindFirst("id")?.Value;
        using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));

        var data = await con.QueryAsync(
            "select id, bank_name, ifsc, branch from bank_accounts where user_id=@UserId",
            new { UserId = userId });

        return Ok(data);
    }
}
