using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CardController : ControllerBase
{
    private readonly IConfiguration _config;

    public CardController(IConfiguration config)
    {
        _config = config;
    }

    // ✅ Safe user id extraction (int)
    private int GetUserId()
    {
        var idStr =
            User.FindFirst("id")?.Value ??
            User.FindFirst("userId")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new Exception("UserId claim not found in JWT");

        if (!int.TryParse(idStr, out var id))
            throw new Exception("UserId claim is not integer");

        return id;
    }

    [HttpPost]
    public async Task<IActionResult> Add(CardModel model)
    {
        try
        {
            var userId = GetUserId();

            using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
            await con.ExecuteAsync(
                @"insert into cards(user_id, holder_name, card_type, last4, exp_month, exp_year)
                  values(@UserId, @HolderName, @CardType, @Last4, @ExpMonth, @ExpYear)",
                new
                {
                    UserId = userId,
                    model.HolderName,
                    model.CardType,
                    model.Last4,
                    model.ExpMonth,
                    model.ExpYear
                });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);  // 👈 real error if any
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
                "select id, holder_name, card_type, last4, exp_month, exp_year from cards where user_id=@UserId",
                new { UserId = userId });

            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);  // 👈 real error if any
        }
    }
}
