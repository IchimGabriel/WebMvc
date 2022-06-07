using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMvc.Data;
using WebMvc.Models;

namespace WebMvc.Controllers
{
    public class ShopsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ShopsController(ApplicationDbContext context, UserManager<IdentityUser> userMng)
        {
            _context = context;
            _userManager = userMng; 
        }

        // GET: Shops
        public async Task<IActionResult> Index()
        {
              return _context.Shops != null ? 
                          View(await _context.Shops.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Shops'  is null.");
        }

        // GET: Shops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Shops == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .FirstOrDefaultAsync(m => m.ShopId == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        // GET: Shops/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShopId,ShopIdentity,Name,Open")] Shop shop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        // GET: Shops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Shops == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }
            return View(shop);
        }

        // POST: Shops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShopId,ShopIdentity,Name,Open")] Shop shop)
        {
            if (id != shop.ShopId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.ShopId))
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
            return View(shop);
        }

        // GET: Shops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Shops == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .FirstOrDefaultAsync(m => m.ShopId == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }

        // POST: Shops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Shops == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Shops'  is null.");
            }
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                _context.Shops.Remove(shop);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShopExists(int id)
        {
          return (_context.Shops?.Any(e => e.ShopId == id)).GetValueOrDefault();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ShopClose([Bind("ShopId,ShopIdentity,Name,Open")] Shop shop)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
                var meshop = _context.Shops.Where(s => s.ShopIdentity.Equals(user)).ToList();
                var meid = meshop[0].ShopId;
                var name = meshop[0].Name;

                var currentshop = _context.Shops.Find(meid);

                currentshop.ShopIdentity = user;
                currentshop.Name = name;
                currentshop.Open = false;


                _context.Entry(currentshop).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Orders");
            }

            return RedirectToAction("Index", "Orders");
        }

        [HttpPost] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShopOpen([Bind("ShopId,ShopIdentity,Name,Open")] Shop shop)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
                var meshop = _context.Shops.Where(s => s.ShopIdentity.Equals(user)).ToList();
                var meid = meshop[0].ShopId;
                var name = meshop[0].Name;

                var currentshop = _context.Shops.Find(meid);

                currentshop.ShopIdentity = user;
                currentshop.Name = name;
                currentshop.Open = true;


                _context.Entry(currentshop).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Orders");
            }

            return RedirectToAction("Index", "Orders");
        }
    }
}
