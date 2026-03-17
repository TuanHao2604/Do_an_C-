using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using TravelGuideWebAdmin.Data;
using TravelGuideWebAdmin.Models;

namespace TravelGuideWebAdmin.Controllers
{
    [Authorize]
    public class POIsController : Controller
    {
        private readonly TravelGuideContext _context;

        public POIsController(TravelGuideContext context)
        {
            _context = context;
        }

        // GET: POIs
        public async Task<IActionResult> Index()
        {
            return View(await _context.POIs.ToListAsync());
        }

        // GET: POIs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pOI = await _context.POIs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pOI == null)
            {
                return NotFound();
            }

            return View(pOI);
        }

        // GET: POIs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: POIs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,DescriptionEn,Latitude,Longitude,Radius,Priority")] POI pOI)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pOI);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pOI);
        }

        // GET: POIs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pOI = await _context.POIs.FindAsync(id);
            if (pOI == null)
            {
                return NotFound();
            }
            return View(pOI);
        }

        // POST: POIs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DescriptionEn,Latitude,Longitude,Radius,Priority")] POI pOI)
        {
            if (id != pOI.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pOI);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!POIExists(pOI.Id))
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
            return View(pOI);
        }

        // GET: POIs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pOI = await _context.POIs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pOI == null)
            {
                return NotFound();
            }

            return View(pOI);
        }

        // POST: POIs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pOI = await _context.POIs.FindAsync(id);
            if (pOI != null)
            {
                _context.POIs.Remove(pOI);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool POIExists(int id)
        {
            return _context.POIs.Any(e => e.Id == id);
        }

        // GET: POIs/Qr/5
        public async Task<IActionResult> Qr(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var poi = await _context.POIs.FirstOrDefaultAsync(p => p.Id == id);
            if (poi == null)
            {
                return NotFound();
            }

            var payload = $"POI-{poi.Id}";
            var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(data);
            var pngBytes = qrCode.GetGraphic(20);

            return File(pngBytes, "image/png", $"poi-{poi.Id}.png");
        }
    }
}
