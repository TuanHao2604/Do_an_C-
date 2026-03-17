using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGuideWebAdmin.Data;
using TravelGuideWebAdmin.Models;

namespace TravelGuideWebAdmin.Controllers.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiBearer")]
    [Route("api/tours")]
    public class ToursApiController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public ToursApiController(TravelGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetAll()
        {
            var items = await _context.Tours.AsNoTracking().ToListAsync();
            return Ok(items);
        }
    }
}
