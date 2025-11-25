using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/v1/error-log")]
    public class ErrorLogController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ErrorLogController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/v1/error-log
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetErrorLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? entityId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _context.ErrorLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityId))
            {
                query = query.Where(e => e.EntityId != null && e.EntityId.Contains(entityId));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(e => e.Timestamp <= toDate.Value);
            }

            var totalCount = await query.CountAsync();
            var errorLogs = await query
                .OrderByDescending(e => e.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return errorLogs;
        }

        // DELETE: api/v1/error-log/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteErrorLog(int id)
        {
            try
            {
                var errorLog = await _context.ErrorLogs.FindAsync(id);
                if (errorLog == null)
                {
                    return NotFound(new { error = $"Лог с id {id} не найден" });
                }

                _context.ErrorLogs.Remove(errorLog);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await _context.LogErrorToBd("DeleteLog", ex, "Log", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());
                return StatusCode(500, new { error = "Ошибка сервера при удалении лога", details = ex.Message });
            }
        }
    }
}
