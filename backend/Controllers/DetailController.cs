using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/v1/detail")]
    public class DetailController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public DetailController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/v1/detail
        [HttpGet]
        public async Task<ActionResult<List<DetailGetDto>>> GetDetails()
        {
            var details = await _context.Details
                .AsNoTracking()
                .Select(d => new DetailGetDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Amount = d.Amount,
                    MasterId = d.MasterId
                })
                .ToListAsync();

            return Ok(details);
        }

        // GET: api/v1/detail/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DetailGetByIdDto>> GetDetail(int id)
        {
            var detail = await _context.Details
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DetailGetByIdDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Amount = d.Amount,
                    MasterId = d.MasterId,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (detail == null)
            {
                return NotFound(new { error = $"Спецификация с ID {id} не найдена" });
            }

            return Ok(detail);
        }
        // POST: api/v1/detail/{masterId}
        [HttpPost("{masterId}")]
        public async Task<ActionResult> CreateDetails(int masterId, [FromBody] DetailBulkCreateDto request)
        {
            try
            {
                var master = await _context.Masters.FindAsync(masterId);

                if (master == null)
                {
                    return NotFound(new { error = $"Документ с id {masterId} не найден" });
                }

                var details = request.Details.Select(d => new Detail
                {
                    Name = d.Name,
                    Amount = d.Amount,
                    MasterId = masterId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.Details.AddRangeAsync(details);
                await _context.SaveChangesAsync();

                await RecalculateMasterAmountAsync(masterId);

                var createdDetails = details.Select(d => new DetailGetDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Amount = d.Amount,
                    MasterId = d.MasterId
                }).ToList();

                return Ok(createdDetails);
            }
            catch (Exception ex)
            {
                await _context.LogErrorToBd("CreateDetails", ex, "Detail", masterId.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());
                return StatusCode(500, new { error = "Ошибка сервера при создании спецификаций", details = ex.Message });
            }
        }

        // PUT: api/v1/detail/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<DetailGetByIdDto>> UpdateDetail(int id, [FromBody] DetailUpdateDto request)
        {
            var detail = await _context.Details
                .Include(d => d.Master)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detail == null)
            {
                return NotFound(new { error = $"Деталь с id {id} не найдена" });
            }

            var oldAmount = detail.Amount;
            var amountDifference = request.Amount - oldAmount;

            detail.Name = request.Name;
            detail.Amount = request.Amount;
            detail.UpdatedAt = DateTime.UtcNow;

            await RecalculateMasterAmountAsync(detail.MasterId);

            await _context.SaveChangesAsync();

            var updatedDetail = new DetailGetByIdDto
            {
                Id = detail.Id,
                Name = detail.Name,
                Amount = detail.Amount,
                MasterId = detail.MasterId,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt
            };

            return Ok(updatedDetail);
        }

        // DELETE: api/v1/detail/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetail(int id)
        {
            try
            {
                var detail = await _context.Details.FindAsync(id);
                if (detail == null)
                {
                    return NotFound(new { error = $"Деталь с id {id} не найдена" });
                }

                var masterId = detail.MasterId;

                _context.Details.Remove(detail);
                await _context.SaveChangesAsync();

                await RecalculateMasterAmountAsync(masterId);

                return Ok();
            }
            catch (Exception ex)
            {
                await _context.LogErrorToBd("DeleteMaster", ex, "Master", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());
                return StatusCode(500, new { error = "Ошибка сервера при удалении спецификации", details = ex.Message });
            }
        }

        private async Task RecalculateMasterAmountAsync(int masterId)
        {
            var master = await _context.Masters
                .Include(m => m.Details)
                .FirstOrDefaultAsync(m => m.Id == masterId);

            if (master != null)
            {
                master.Amount = master.Details.Sum(d => d.Amount);
                master.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}