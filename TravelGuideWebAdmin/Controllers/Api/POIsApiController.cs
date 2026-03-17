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

        // GET api/pois
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAll()
        {
            var items = await _context.POIs.AsNoTracking().ToListAsync();
            return Ok(items);
        }

        // GET api/pois/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<POI>> GetById(int id)
        {
            var item = await _context.POIs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // GET api/pois/5/images
        [HttpGet("{id:int}/images")]
        public async Task<ActionResult<IEnumerable<POI_Image>>> GetImages(int id)
        {
            var images = await _context.POI_Images.AsNoTracking()
                .Where(i => i.PoiId == id).ToListAsync();
            return Ok(images);
        }

        // GET api/pois/5/media
        [HttpGet("{id:int}/media")]
        public async Task<ActionResult<IEnumerable<POI_Media>>> GetMedia(int id)
        {
            var media = await _context.POI_Medias.AsNoTracking()
                .Where(m => m.PoiId == id).ToListAsync();
            return Ok(media);
        }

        // POST api/pois
        [HttpPost]
        public async Task<ActionResult<POI>> Create(POI poi)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            poi.Id = 0;
            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
        }

        // PUT api/pois/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, POI poi)
        {
            if (id != poi.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(poi).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.POIs.AnyAsync(p => p.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE api/pois/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.POIs.FindAsync(id);
            if (item == null) return NotFound();
            _context.POIs.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
