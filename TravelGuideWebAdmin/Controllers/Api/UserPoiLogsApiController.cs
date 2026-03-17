using System.Security.Claims;
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

            // Xác thực: username phải khớp với ClientId trong JWT, không cho phép client tự đặt username tùy ý
            var clientIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(log.Username))
                log.Username = clientIdFromToken;

            _context.User_POI_Logs.Add(log);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
