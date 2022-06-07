using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebMvc.Data;
using WebMvc.Models;

namespace WebMvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        /// <summary>
        /// ORDERS ISSUED BY CURRENT SHOP
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "ShopMng")]
        public async Task<ActionResult> Index()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
            var orders = _context.Orders
                .Where(s => s.ShopIdentity.Equals(user) && s.DriverIdentity == null)
                .OrderByDescending(t => t.TimeStamp);

            var shop = _context.Shops
                .Where(s => s.ShopIdentity.Equals(user))
                .ToList();

            var isOpen = shop[0].Open;

            ViewBag.Open = isOpen;

            return View(await orders.ToListAsync());
        }

        /// <summary>
        /// CURRENT SHOP ORDERS -> ON ROUTE TO CUSTOMERS
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "ShopMng")]
        public async Task<ActionResult> OnDelivery()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;

            var ondelivery = _context.Orders
                .Where(s => s.DriverIdentity.Length > 1 && s.ShopIdentity.Equals(user) && s.IsDelivered == false)
                .OrderByDescending(t => t.TimeStamp);

            return View(await ondelivery.ToListAsync());
        }

        /// <summary>
        /// CURRENT SHOP ORDERS -> DELIVERED
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "ShopMng")]
        public async Task<ActionResult> Delivered()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;

            var delivered = _context.Orders.Where(s => s.IsDelivered.Equals(true) && s.ShopIdentity.Equals(user));

            return View(await delivered.ToListAsync());
        }

        /// <summary>
        /// ORDERS FROM ALL SHOPS -> ADMIN AUTHORIZATION
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "ShopMng")]
        public ActionResult Statistics()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
            var orders = _context.Orders.Where(s => s.ShopIdentity.Equals(user));

            if (orders.Count() == 0)
            {
                ViewBag.Message = "There are no Orders";
            }
            else
            {
                ViewBag.OrdersCount = orders.Count();
                ViewBag.TotalValue = orders.Sum(s => s.Total);
                ViewBag.TotalCommision = orders.Sum(s => s.Commission);
                ViewBag.ShopTotal = orders.Sum(s => s.Total) - orders.Sum(s => s.Commission);
            }

            return View();
        }

        

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.DriverId = new SelectList(_context.Drivers, "DriverIdentity", "Name");
            ViewBag.ShopId = new SelectList(_context.Shops, "ShopIdentity", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,TimeStamp,Total,Commission,Address,IsDelivered,DriverIdentity,ShopIdentity")] Order order)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
                order.ShopIdentity = user;
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.DriverId = new SelectList(_context.Drivers, "DriverIdentity", "Name", order.DriverIdentity);
            ViewBag.ShopId = new SelectList(_context.Shops, "ShopIdentity", "Name", order.ShopIdentity);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,TimeStamp,Total,Commission,Address,IsDelivered,DriverIdentity,ShopIdentity")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }
    }
}
