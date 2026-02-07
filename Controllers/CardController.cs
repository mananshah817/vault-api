using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;

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

    [HttpPost]
    public async Task<IActionResult> Add(CardModel model)
    {
        var userId = User.FindFirst("id")?.Value;

        using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
        await con.ExecuteAsync(
            @"insert into cards(user_id, holder_name, card_type, last4, exp_month, exp_year)
              values(@UserId, @HolderName, @CardType, @Last4, @ExpMonth, @ExpYear)",
            new { UserId = userId, model.HolderName, model.CardType, model.Last4, model.ExpMonth, model.ExpYear });

        return Ok("Saved");
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = User.FindFirst("id")?.Value;

        using var con = new NpgsqlConnection(_config.GetConnectionString("Default"));
        var data = await con.QueryAsync(
            "select id, holder_name, card_type, last4, exp_month, exp_year from cards where user_id=@UserId",
            new { UserId = userId });

        return Ok(data);
    }
}
