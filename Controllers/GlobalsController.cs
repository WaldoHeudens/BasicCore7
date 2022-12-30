using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BasicCore7.Data;
using BasicCore7.Models;
using Microsoft.AspNetCore.Authorization;
using BasicCore7.Services;

namespace BasicCore7.Controllers
{
    [Authorize (Roles = "SystemAdministrator")]
    public class GlobalsController : Controller
    {
        private readonly BasicCore7DbContext _context;

        public GlobalsController(BasicCore7DbContext context)
        {
            _context = context;
        }

        // GET: Globals
        public async Task<IActionResult> Index()
        {
              return _context.Globals != null ? 
                          View(await _context.Globals.ToListAsync()) :
                          Problem("Entity set 'BasicCore7DbContext.Globals'  is null.");
        }

        // GET: Globals/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Globals == null)
            {
                return NotFound();
            }

            var @global = await _context.Globals
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@global == null)
            {
                return NotFound();
            }

            return View(@global);
        }

        // GET: Globals/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Globals == null)
            {
                return NotFound();
            }

            var @global = await _context.Globals.FindAsync(id);
            if (@global == null)
            {
                return NotFound();
            }
            return View(@global);
        }

        // POST: Globals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Value,Description")] Global @global)
        {
            if (id != @global.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@global);
                    await _context.SaveChangesAsync();
                    Globals.ApplyGlobals(_context);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GlobalExists(@global.Id))
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
            return View(@global);
        }


        private bool GlobalExists(string id)
        {
          return (_context.Globals?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
