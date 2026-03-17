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
    [Route("api/pois")]
    public class POIsApiController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public POIsApiController(TravelGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAll()
        {
            var items = await _context.POIs.AsNoTracking().ToListAsync();
            return Ok(items);
        }
    }
}
