using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripApp.Data;
using TripApp.Models.DTOs;

namespace TripApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripsController(ApbdContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Include(t => t.IdCountries)
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .AsQueryable();

        var totalTrips = await query.CountAsync();
        var trips = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDto
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            })
            .ToListAsync();

        var response = new GetTrips
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            Trips = trips
        };

        return Ok(response);
    }
}