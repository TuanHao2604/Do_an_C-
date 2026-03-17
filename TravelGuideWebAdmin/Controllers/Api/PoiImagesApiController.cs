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
    [Route("api/poi-images")]
    public class PoiImagesApiController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public PoiImagesApiController(TravelGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI_Image>>> GetAll()
        {
            var items = await _context.POI_Images.AsNoTracking().ToListAsync();
            return Ok(items);
        }
    }
}
