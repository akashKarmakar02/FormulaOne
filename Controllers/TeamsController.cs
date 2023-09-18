using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormulaOne.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{

    private static AppDbContext _context;

    public TeamsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var teams = await _context.Teams.ToListAsync();
        return Ok(teams);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.id == id);

        if (team == null)
            return BadRequest("Invalid Id");

        return Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Team team)
    {
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction("Get", team.id, team);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch(int id, string country)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.id == id);

        if (team == null)
            return BadRequest("Invalid Id");

        team.Country = country;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.id == id);

        if (team == null)
            return BadRequest("Invalid id");

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}