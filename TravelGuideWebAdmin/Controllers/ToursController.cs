using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelGuideWebAdmin.Data;
using TravelGuideWebAdmin.Models;

namespace TravelGuideWebAdmin.Controllers
{
    [Authorize]
    public class ToursController : Controller
    {
        private readonly TravelGuideContext _context;

        public ToursController(TravelGuideContext context)
        {
            _context = context;
        }

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tours.ToListAsync());
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tour == null)
            {
                return NotFound();
            }

            var mappedPoiIds = await _context.Tour_POIs
                .Where(tp => tp.TourId == tour.Id)
                .Select(tp => tp.PoiId)
                .ToListAsync();

            ViewBag.MappedPoiIds = mappedPoiIds;
            ViewBag.AllPois = await _context.POIs.ToListAsync();

            return View(tour);
        }

        // POST: Tours/AddPoi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPoi(int tourId, int poiId)
        {
            var existing = await _context.Tour_POIs
                .FirstOrDefaultAsync(tp => tp.TourId == tourId && tp.PoiId == poiId);
            if (existing != null)
            {
                return RedirectToAction(nameof(Details), new { id = tourId });
            }

            var nextIndex = await _context.Tour_POIs
                .Where(tp => tp.TourId == tourId)
                .CountAsync();

            _context.Tour_POIs.Add(new Tour_POI
            {
                TourId = tourId,
                PoiId = poiId,
                OrderIndex = nextIndex + 1
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = tourId });
        }

        // POST: Tours/RemovePoi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePoi(int tourId, int poiId)
        {
            var existing = await _context.Tour_POIs
                .FirstOrDefaultAsync(tp => tp.TourId == tourId && tp.PoiId == poiId);
            if (existing != null)
            {
                _context.Tour_POIs.Remove(existing);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = tourId });
        }

        // GET: Tours/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,DescriptionEn,EstimatedTime")] Tour tour)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }
            return View(tour);
        }

        // POST: Tours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DescriptionEn,EstimatedTime")] Tour tour)
        {
            if (id != tour.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tour);
        }

        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tour == null)
            {
                return NotFound();
            }

            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.Id == id);
        }
    }
}
