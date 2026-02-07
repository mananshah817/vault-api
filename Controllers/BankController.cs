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

    private int GetUserId()
{
    var idStr =
        User.FindFirst("id")?.Value ??
        User.FindFirst("userId")?.Value ??
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new Exception("UserId claim not found in JWT");

    if (!int.TryParse(idStr, out var id))
        throw new Exception("UserId claim is not integer");

    return id;
}
    [HttpPost]
    public async Task<IActionResult> Add(BankModel model)
    {
        try
        {
            var userId = GetUserId();
            var key = _config["Crypto:Key"] ?? throw new Exception("Crypto:Key missing");
            var encAcc = CryptoHelper.Encrypt(model.AccountNumber, key);

            using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
            await con.ExecuteAsync(
                @"insert into bank_accounts(user_id, bank_name, account_enc, ifsc, branch)
                  values(@UserId, @BankName, @AccountEnc, @IFSC, @Branch)",
                new { UserId = userId, model.BankName, AccountEnc = encAcc, model.IFSC, model.Branch });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message); // 👈 real error dikhega
        }
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        try
        {
            var userId = GetUserId();

            using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
            var data = await con.QueryAsync(
                "select id, bank_name, ifsc, branch from bank_accounts where user_id=@UserId",
                new { UserId = userId });

            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message); // 👈 real error dikhega
        }
    }
}
