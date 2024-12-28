using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSM311.Data;
using BSM311.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using BSM311.Models.DTOs;
using System.Linq;
[Area("Admin")]
[ApiController]
[Route("api/[area]/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/admin/employee
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetEmployees()
    {
        try
        {
            var employees = await _context.Employees
           .Include(e => e.WorkDays)
           .Include(e => e.Expertises)
               .ThenInclude(ee => ee.Expertise) 
           .Select(e => new EmployeeDTO
           {
               Id = e.Id,
               FirstName = e.FirstName,
               LastName = e.LastName,
               WorkDays = e.WorkDays.Select(wd => wd.DayOfWeek).ToList(),
               ExpertiseNames = e.Expertises.Select(ex => ex.Expertise.Name).ToList() 
           })
           .ToListAsync();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }





    // GET: api/admin/employee/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDTO>> Get(int id)
    {
        var employee = await (from e in _context.Employees
                              where e.Id == id
                              select new EmployeeDTO
                              {
                                  Id = e.Id,
                                  FirstName = e.FirstName,
                                  LastName = e.LastName,
                                  WorkDays = e.WorkDays.Select(w => w.DayOfWeek).ToList(),
                                  ExpertiseNames = e.Expertises.Select(ex => ex.Expertise.Name).ToList()
                              }).FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        return employee;
    }


    // POST: api/admin/employee
    [HttpPost]
    public async Task<ActionResult<EmployeeDTO>> AddEmployee([FromBody] EmployeeDTO employeeDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var expertises = new List<Expertise>();
            if (employeeDto.ExpertiseNames.Any())
            {
                expertises = await _context.Expertises
                    .Where(e => employeeDto.ExpertiseNames.Contains(e.Name))
                    .ToListAsync();

                if (expertises.Count != employeeDto.ExpertiseNames.Count)
                {
                    var foundNames = expertises.Select(e => e.Name);
                    var notFound = employeeDto.ExpertiseNames.Except(foundNames);
                    return BadRequest(new { message = $"Some expertises were not found: {string.Join(", ", notFound)}" });
                }
            }

            var employee = new Employee
            {
                FirstName = employeeDto.FirstName.Trim(),
                LastName = employeeDto.LastName.Trim(),
                WorkDays = employeeDto.WorkDays.Select(day => new EmployeeWorkDay
                {
                    DayOfWeek = day,
                    IsWorking = true
                }).ToList()
            };

            // EmployeeExpertise nesnelerini oluştururken Employee özelliğini ayarlayın
            employee.Expertises = expertises.Select(e => new EmployeeExpertise
            {
                ExpertiseId = e.Id,
                Expertise = e,
                Employee = employee // Burada Employee özelliği dolduruluyor
            }).ToList();

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            employeeDto.Id = employee.Id;
            employeeDto.ExpertiseNames = expertises.Select(e => e.Name).ToList();

            return CreatedAtAction(nameof(Get), new { id = employee.Id }, employeeDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }






    // PUT: api/admin/employee/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeDTO updatedEmployeeDto)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.WorkDays)
                .Include(e => e.Expertises)
                .ThenInclude(ee => ee.Expertise)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            employee.FirstName = updatedEmployeeDto.FirstName.Trim();
            employee.LastName = updatedEmployeeDto.LastName.Trim();

            employee.WorkDays.Clear();
            foreach (var day in updatedEmployeeDto.WorkDays)
            {
                employee.WorkDays.Add(new EmployeeWorkDay
                {
                    EmployeeId = id,
                    DayOfWeek = day,
                    IsWorking = true
                });
            }

            if (updatedEmployeeDto.ExpertiseNames.Any())
            {
                var expertises = await _context.Expertises
                    .Where(e => updatedEmployeeDto.ExpertiseNames.Contains(e.Name))
                    .ToListAsync();

                if (expertises.Count != updatedEmployeeDto.ExpertiseNames.Count)
                {
                    var foundNames = expertises.Select(e => e.Name);
                    var notFound = updatedEmployeeDto.ExpertiseNames.Except(foundNames);
                    return BadRequest(new { message = $"Some expertises were not found: {string.Join(", ", notFound)}" });
                }

                employee.Expertises.Clear();
                foreach (var expertise in expertises)
                {
                    employee.Expertises.Add(new EmployeeExpertise
                    {
                        EmployeeId = id,
                        ExpertiseId = expertise.Id,
                        Expertise = expertise,
                        Employee = employee // Burada Employee özelliği dolduruluyor
                    });
                }
            }
            else
            {
                employee.Expertises.Clear();
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }






    private async Task<bool> EmployeeExists(int id)
    {
        return await _context.Employees.AnyAsync(e => e.Id == id);
    }

    // DELETE: api/admin/employee/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await (from e in _context.Employees
                              where e.Id == id
                              select e).FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        try
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET: api/admin/employee/{id}/workdays
    [HttpGet("{id}/workdays")]
    public async Task<ActionResult<IEnumerable<DayOfWeek>>> GetWorkDays(int id)
    {
        var workDays = await (from w in _context.EmployeeWorkDays
                              where w.EmployeeId == id
                              select w.DayOfWeek).ToListAsync();

        if (!workDays.Any())
            return NotFound(new { message = "No work days found" });

        return Ok(workDays);
    }

    // PUT: api/admin/employee/{id}/workdays
    [HttpPut("{id}/workdays")]
    public async Task<IActionResult> UpdateWorkDays(int id, [FromBody] List<DayOfWeek> workDays)
    {
        var employee = await (from e in _context.Employees
                              where e.Id == id
                              select e).FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        try
        {
            employee.WorkDays = workDays.Select(day => new EmployeeWorkDay
            {
                EmployeeId = id,
                DayOfWeek = day,
                IsWorking = true
            }).ToList();

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}