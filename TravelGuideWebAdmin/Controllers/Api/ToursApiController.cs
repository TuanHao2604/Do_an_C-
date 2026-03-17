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

        // GET api/tours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetAll()
        {
            var items = await _context.Tours.AsNoTracking().ToListAsync();
            return Ok(items);
        }

        // GET api/tours/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tour>> GetById(int id)
        {
            var item = await _context.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // GET api/tours/5/pois — danh sách POI của tour theo thứ tự
        [HttpGet("{id:int}/pois")]
        public async Task<ActionResult<IEnumerable<POI>>> GetPois(int id)
        {
            var mappings = await _context.Tour_POIs.AsNoTracking()
                .Where(tp => tp.TourId == id)
                .OrderBy(tp => tp.OrderIndex)
                .ToListAsync();

            if (mappings.Count == 0) return Ok(new List<POI>());

            var poiIds = mappings.Select(m => m.PoiId).ToHashSet();
            var pois   = await _context.POIs.AsNoTracking()
                .Where(p => poiIds.Contains(p.Id))
                .ToListAsync();

            var map    = pois.ToDictionary(p => p.Id, p => p);
            var result = mappings
                .Where(m => map.ContainsKey(m.PoiId))
                .Select(m => map[m.PoiId])
                .ToList();

            return Ok(result);
        }

        // POST api/tours
        [HttpPost]
        public async Task<ActionResult<Tour>> Create(Tour tour)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            tour.Id = 0;
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tour.Id }, tour);
        }

        // PUT api/tours/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Tour tour)
        {
            if (id != tour.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(tour).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Tours.AnyAsync(t => t.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE api/tours/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Tours.FindAsync(id);
            if (item == null) return NotFound();
            _context.Tours.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST api/tours/5/pois — thêm POI vào tour
        [HttpPost("{id:int}/pois")]
        public async Task<IActionResult> AddPoi(int id, [FromBody] TourPoiRequest req)
        {
            if (!await _context.Tours.AnyAsync(t => t.Id == id)) return NotFound("Tour not found.");
            if (!await _context.POIs.AnyAsync(p => p.Id == req.PoiId))  return NotFound("POI not found.");

            var exists = await _context.Tour_POIs.AnyAsync(tp => tp.TourId == id && tp.PoiId == req.PoiId);
            if (exists) return Conflict("POI already in tour.");

            _context.Tour_POIs.Add(new Tour_POI
            {
                TourId     = id,
                PoiId      = req.PoiId,
                OrderIndex = req.OrderIndex
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/tours/5/pois/3 — xóa POI khỏi tour
        [HttpDelete("{id:int}/pois/{poiId:int}")]
        public async Task<IActionResult> RemovePoi(int id, int poiId)
        {
            var mapping = await _context.Tour_POIs
                .FirstOrDefaultAsync(tp => tp.TourId == id && tp.PoiId == poiId);
            if (mapping == null) return NotFound();

            _context.Tour_POIs.Remove(mapping);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class TourPoiRequest
    {
        public int PoiId      { get; set; }
        public int OrderIndex { get; set; }
    }
}
