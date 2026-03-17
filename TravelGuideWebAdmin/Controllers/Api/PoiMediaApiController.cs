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
    [Route("api/poi-media")]
    public class PoiMediaApiController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public PoiMediaApiController(TravelGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI_Media>>> GetAll()
        {
            var items = await _context.POI_Medias.AsNoTracking().ToListAsync();
            return Ok(items);
        }
    }
}
