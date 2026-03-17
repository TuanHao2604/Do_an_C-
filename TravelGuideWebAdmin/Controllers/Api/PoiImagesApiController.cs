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

        // GET api/poi-images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI_Image>>> GetAll()
        {
            var items = await _context.POI_Images.AsNoTracking().ToListAsync();
            return Ok(items);
        }

        // GET api/poi-images/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<POI_Image>> GetById(int id)
        {
            var item = await _context.POI_Images.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST api/poi-images
        [HttpPost]
        public async Task<ActionResult<POI_Image>> Create(POI_Image image)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _context.POIs.AnyAsync(p => p.Id == image.PoiId))
                return BadRequest("POI not found.");

            image.Id = 0;
            _context.POI_Images.Add(image);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = image.Id }, image);
        }

        // PUT api/poi-images/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, POI_Image image)
        {
            if (id != image.Id) return BadRequest();
            _context.Entry(image).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.POI_Images.AnyAsync(i => i.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE api/poi-images/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.POI_Images.FindAsync(id);
            if (item == null) return NotFound();
            _context.POI_Images.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
