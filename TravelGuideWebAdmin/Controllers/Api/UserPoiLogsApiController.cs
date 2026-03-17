using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelGuideWebAdmin.Data;
using TravelGuideWebAdmin.Models;

namespace TravelGuideWebAdmin.Controllers.Api
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiBearer")]
    [Route("api/logs")]
    public class UserPoiLogsApiController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public UserPoiLogsApiController(TravelGuideContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(User_POI_Log log)
        {
            if (log == null || log.PoiId == 0)
                return BadRequest();

            _context.User_POI_Logs.Add(log);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
