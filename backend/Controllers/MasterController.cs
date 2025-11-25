using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/v1/master")]
    public class MasterController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public MasterController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/v1/master?sortBy=date&sortOrder=desc&search=DOC
        [HttpGet]
        public async Task<ActionResult<List<MasterGetDto>>> GetAllMasters(
            [FromQuery] string? search = null,
            [FromQuery] string sortBy = "date",
            [FromQuery] string sortOrder = "desc")
        {
            var query = _context.Masters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(m =>
                    m.Number.ToLower().Contains(searchLower) ||
                    (m.Note != null && m.Note.ToLower().Contains(searchLower))
                );
            }

            query = sortBy.ToLower() switch
            {
                "number" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(m => m.Number)
                    : query.OrderByDescending(m => m.Number),

                "amount" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(m => m.Amount)
                    : query.OrderByDescending(m => m.Amount),

                "date" or _ => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(m => m.Date)
                    : query.OrderByDescending(m => m.Date)
            };

            var result = await query
                .Select(m => new MasterGetDto
                {
                    Id = m.Id,
                    Number = m.Number,
                    Date = m.Date,
                    Amount = m.Amount,
                    Note = m.Note
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/v1/master/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MasterGetByIdDto>> GetById(int id)
        {
            try
            {
                var masterDto = await _context.Masters
                    .AsNoTracking()
                    .Where(m => m.Id == id)
                    .Select(m => new MasterGetByIdDto
                    {
                        Id = m.Id,
                        Number = m.Number,
                        Date = m.Date,
                        Amount = m.Amount,
                        Note = m.Note,
                        Details = m.Details.Select(d => new DetailDto
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Amount = d.Amount
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (masterDto == null)
                {
                    return NotFound(new { error = $"Документ с id {id} не найден" });
                }

                return Ok(masterDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка сервера при получении документа с полной информацией", details = ex.Message });
            }
        }

        // POST: api/v1/master
        [HttpPost]
        public async Task<ActionResult<MasterGetByIdDto>> CreateMaster(MasterCreateDto createDto)
        {
            try
            {
                if (await _context.Masters.AnyAsync(m => m.Number == createDto.Number))
                {
                    var errorMessage = $"Документ с номером '{createDto.Number}' уже существует";
                    await _context.LogErrorToBd("CreateMaster", new InvalidOperationException(errorMessage), "Master", createDto.Number, HttpContext.Connection.RemoteIpAddress?.ToString());
                    return Conflict(new { error = errorMessage });
                }

                var master = new Master
                {
                    Number = createDto.Number,
                    Date = createDto.Date,
                    Note = createDto.Note,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (createDto.Details != null && createDto.Details.Any())
                {
                    foreach (var detailDto in createDto.Details)
                    {
                        var detail = new Detail
                        {
                            Name = detailDto.Name,
                            Amount = detailDto.Amount,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        master.Details.Add(detail);
                    }

                    master.Amount = master.Details.Sum(d => d.Amount);
                }

                _context.Masters.Add(master);
                await _context.SaveChangesAsync();

                var result = new MasterGetByIdDto
                {
                    Id = master.Id,
                    Number = master.Number,
                    Date = master.Date,
                    Amount = master.Amount,
                    Note = master.Note,
                    Details = master.Details.Select(d => new DetailDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Amount = d.Amount
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetById), new { id = master.Id }, result); ;
            }
            catch (Exception ex)
            {
                await _context.LogErrorToBd("CreateMaster", ex, "Master", createDto.Number, HttpContext.Connection.RemoteIpAddress?.ToString());
                return StatusCode(500, new { error = "Ошибка сервера при создании документа", details = ex.Message });
            }
        }
    }
}
