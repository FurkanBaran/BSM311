using Microsoft.AspNetCore.Mvc;
using BSM311.Data;
using BSM311.Models;
using BSM311.Models.DTOs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BSM311.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;
[Area("Admin")]
[Route("api/[area]/[controller]")]
[ApiController]
[Authorize(Roles = UserRoles.Admin)]
public class ExpertiseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExpertiseController(ApplicationDbContext context)
    {
        _context = context;
    }
    // GET: api/admin/expertise
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpertiseDTO>>> GetExpertises()
    {
        try
        {
            var expertises = await (from e in _context.Expertises
                                  .Include(e => e.Services)
                                    select new ExpertiseDTO
                                    {
                                        Id = e.Id,
                                        Name = e.Name,
                                        Description = e.Description,
                                        Services = e.Services.Select(s => new ServiceDTO
                                        {
                                            Id = s.Id,
                                            Name = s.Name,
                                            Description = s.Description,
                                            Price = s.Price,
                                            DurationInMinutes = s.DurationInMinutes
                                        }).ToList()
                                    }).ToListAsync();

            return Ok(expertises);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/admin/expertise/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ExpertiseDTO>> GetExpertise(int id)
    {
        var expertise = await (from e in _context.Expertises
                               where e.Id == id
                               select new ExpertiseDTO
                               {
                                   Id = e.Id,
                                   Name = e.Name,
                                   Description = e.Description
                               }).FirstOrDefaultAsync();

        if (expertise == null)
            return NotFound(new { message = "Expertise not found" });

        return expertise;
    }

    // POST: api/admin/expertise
    [HttpPost]
    public async Task<ActionResult<ExpertiseDTO>> Create([FromBody] ExpertiseDTO expertiseDto)
    {
        try
        {
            // Check if expertise with same name already exists
            var exists = await (from e in _context.Expertises
                                where e.Name.ToLower() == expertiseDto.Name.ToLower()
                                select e).AnyAsync();

            if (exists)
                return BadRequest(new { message = "An expertise with this name already exists" });

            var expertise = new Expertise
            {
                Name = expertiseDto.Name.Trim(),
                Description = expertiseDto.Description?.Trim()
            };

            await _context.Expertises.AddAsync(expertise);
            await _context.SaveChangesAsync();

            expertiseDto.Id = expertise.Id;
            return CreatedAtAction(nameof(GetExpertise), new { id = expertise.Id }, expertiseDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT: api/admin/expertise/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] ExpertiseDTO expertiseDto)
    {

        try
        {
            // Check if expertise with same name already exists
            var exists = await (from e in _context.Expertises
                                where e.Name.ToLower() == expertiseDto.Name.ToLower()
                                && e.Id != id
                                select e).AnyAsync();

            if (exists)
                return BadRequest(new { message = "An expertise with this name already exists" });

            var expertise = await (from e in _context.Expertises
                                   where e.Id == id
                                   select e).FirstOrDefaultAsync();

            if (expertise == null)
                return NotFound(new { message = "Expertise not found" });

            expertise.Name = expertiseDto.Name.Trim();
            expertise.Description = expertiseDto.Description?.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExpertiseExists(id))
                return NotFound(new { message = "Expertise not found" });
            throw;
        }
    }

    // DELETE: api/admin/expertise/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var expertise = await (from e in _context.Expertises
                               where e.Id == id
                               select e).FirstOrDefaultAsync();

        if (expertise == null)
            return NotFound(new { message = "Expertise not found" });

        // Check if expertise is being used by any services
        var hasServices = await (from s in _context.Services
                                 where s.ExpertiseId == id
                                 select s).AnyAsync();

        if (hasServices)
            return BadRequest(new { message = "Cannot delete expertise that is being used by services" });

        _context.Expertises.Remove(expertise);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> ExpertiseExists(int id)
    {
        return await (from e in _context.Expertises
                      where e.Id == id
                      select e).AnyAsync();
    }
}