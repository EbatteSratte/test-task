using backend.Data;
using backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/v1/detail")]
    public class MasterController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public MasterController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<MasterGetDto>>> GetAllMasters()
        {
            var query = await _context.Masters
                .AsNoTracking()
                .OrderByDescending(m => m.Date)
                .Select(m => new MasterGetDto
                {
                    Id = m.Id,
                    Number = m.Number,
                    Date = m.Date,
                    Amount = m.Amount,
                    Note = m.Note
                })
                .ToListAsync(); 

            return Ok(query);
        }
    }
}
