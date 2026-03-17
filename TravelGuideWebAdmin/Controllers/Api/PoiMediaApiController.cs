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

        // GET api/poi-media
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI_Media>>> GetAll()
        {
            var items = await _context.POI_Medias.AsNoTracking().ToListAsync();
            return Ok(items);
        }

        // GET api/poi-media/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<POI_Media>> GetById(int id)
        {
            var item = await _context.POI_Medias.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST api/poi-media
        [HttpPost]
        public async Task<ActionResult<POI_Media>> Create(POI_Media media)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _context.POIs.AnyAsync(p => p.Id == media.PoiId))
                return BadRequest("POI not found.");

            media.Id = 0;
            _context.POI_Medias.Add(media);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = media.Id }, media);
        }

        // PUT api/poi-media/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, POI_Media media)
        {
            if (id != media.Id) return BadRequest();
            _context.Entry(media).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.POI_Medias.AnyAsync(m => m.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE api/poi-media/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.POI_Medias.FindAsync(id);
            if (item == null) return NotFound();
            _context.POI_Medias.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
