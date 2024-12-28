using Microsoft.AspNetCore.Mvc;
using BSM311.Data;
using BSM311.Models;
using BSM311.Models.DTOs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BSM311.Constants;

namespace BSM311.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/service
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServices()
        {
            try
            {
                var services = await (from s in _context.Services
                                    .Include(s => s.Expertise)
                                      select new ServiceDTO
                                      {
                                          Id = s.Id,
                                          Name = s.Name,
                                          Description = s.Description,
                                          Price = s.Price,
                                          DurationInMinutes = s.DurationInMinutes,
                                          ExpertiseId = s.ExpertiseId,
                                          ExpertiseName = s.Expertise.Name
                                      }).ToListAsync();

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/admin/service
        [HttpPost]
        public async Task<ActionResult<ServiceDTO>> Create([FromBody] ServiceDTO serviceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var service = new Service
                {
                    Name = serviceDto.Name.Trim(),
                    Description = serviceDto.Description?.Trim(),
                    Price = serviceDto.Price,
                    DurationInMinutes = serviceDto.DurationInMinutes,
                    ExpertiseId = serviceDto.ExpertiseId
                };

                await _context.Services.AddAsync(service);
                await _context.SaveChangesAsync();

                serviceDto.Id = service.Id;
                return CreatedAtAction(nameof(GetService), new { id = service.Id }, serviceDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/admin/service/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDTO>> GetService(int id)
        {
            try
            {
                var service = await (from s in _context.Services
                                   .Include(s => s.Expertise)
                                     where s.Id == id
                                     select new ServiceDTO
                                     {
                                         Id = s.Id,
                                         Name = s.Name,
                                         Description = s.Description,
                                         Price = s.Price,
                                         DurationInMinutes = s.DurationInMinutes,
                                         ExpertiseId = s.ExpertiseId,
                                         ExpertiseName = s.Expertise.Name
                                     }).FirstOrDefaultAsync();

                if (service == null)
                    return NotFound(new { message = "Service not found" });

                return service;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: api/admin/service/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] ServiceDTO serviceDto)
        {
            if (id != serviceDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            try
            {
                var service = await (from s in _context.Services
                                     where s.Id == id
                                     select s).FirstOrDefaultAsync();

                if (service == null)
                    return NotFound(new { message = "Service not found" });

                service.Name = serviceDto.Name.Trim();
                service.Description = serviceDto.Description?.Trim();
                service.Price = serviceDto.Price;
                service.DurationInMinutes = serviceDto.DurationInMinutes;
                service.ExpertiseId = serviceDto.ExpertiseId;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExists(id))
                    return NotFound(new { message = "Service not found" });
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/admin/service/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var service = await (from s in _context.Services
                                     where s.Id == id
                                     select s).FirstOrDefaultAsync();

                if (service == null)
                    return NotFound(new { message = "Service not found" });

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task<bool> ServiceExists(int id)
        {
            return await (from s in _context.Services
                          where s.Id == id
                          select s).AnyAsync();
        }
    }

}