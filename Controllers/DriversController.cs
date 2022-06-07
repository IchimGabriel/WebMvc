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
    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DriversController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }


        /// <summary>
        /// ORDERS FROM ALL SHOPS
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Driver")]
        public async Task<ActionResult> AllOrders()
        {
            var orders = _context.Orders.Where(s => s.DriverIdentity == null);

            var user = _userManager.FindByNameAsync(userName: User.Identity.Name).Result.Id;

            var driver = _context.Drivers
                .Where(s => s.DriverIdentity.Equals(user))
                .ToList();

            var isOnline = driver[0].OnLine;

            ViewBag.Online = isOnline;

            return View(await orders.ToListAsync());
        }

        /// <summary>
        /// ORDERS ON ROUTE TO CUSTOMERS FOR CURRENT DRIVER
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Driver")]
        public async Task<ActionResult> OnDelivery()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
            var ondelivery = _context.Orders.Where(s => s.DriverIdentity.Length > 1 && s.DriverIdentity.Equals(user) && s.IsDelivered == false);

            return View(await ondelivery.ToListAsync());
        }

        /// <summary>
        /// DELIVERED ORDERS FOR CURRENT DRIVER
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Roles = "Driver")]
        public async Task<ActionResult> Delivered()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;

            var delivered = _context.Orders.Where(s => s.IsDelivered.Equals(true) && s.DriverIdentity.Equals(user));

             return View(await delivered.ToListAsync()); 
        }

        /// <summary>
        /// GET STATISTICS FOR DRIVER
        /// </summary>
        /// <returns></returns>   
        [HttpGet]
        //[Authorize(Roles = "Driver")]
        public ActionResult Statistics()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
            var orders = _context.Orders.Where(s => s.DriverIdentity.Equals(user));

            if (!orders.Any())
            {
                ViewBag.Message = "There are no Orders";
            }
            else
            {
                ViewBag.OrdersCount = orders.Count();
                ViewBag.TotalValue = orders.Sum(s => s.Total);
                ViewBag.TotalCommision = orders.Sum(s => s.Commission);
            }

            return View();
        }

        /// <summary>
        /// PUT THE DRIVER IN OFFLINE MODE
        /// </summary>
        /// <param name="driver"> OnLine= false;</param>
        /// <returns>CHANGE STATUS FROM TRUE TO FALSE</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DriverOffline([Bind("DriverId,DriverIdentity,Name,OnLine,OnDelivery")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
                var medriver = _context.Drivers.Where(s => s.DriverIdentity.Equals(user)).ToList();
                var meid = medriver[0].DriverId;
                var name = medriver[0].Name;

                var currentdriver = _context.Drivers.Find(meid);

                currentdriver.DriverIdentity = user;
                currentdriver.Name = name;
                currentdriver.OnLine = false;

                _context.Entry(currentdriver).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("AllOrders", "Drivers");
            }

            return RedirectToAction("AllOrders", "Drivers");
        }

        /// <summary>
        /// PUT THE DRIVER IN ONLINE MODE
        /// </summary>
        /// <param name="driver"> OnLine= true;</param>
        /// <returns>CHANGE STATUS FROM FALSE TO TRUE</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DriverOnline([Bind("DriverId,DriverIdentity,Name,OnLine,OnDelivery")] Driver driver)
        {
            
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
                var medriver = _context.Drivers.Where(s => s.DriverIdentity.Equals(user)).ToList();
                var meid = medriver[0].DriverId;
                var name = medriver[0].Name;

                var currentdriver = _context.Drivers.Find(meid);

                currentdriver.DriverIdentity = user;
                currentdriver.Name = name;
                currentdriver.OnLine = true;

                _context.Entry(currentdriver).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("AllOrders", "Drivers");
            }

            return RedirectToAction("AllOrders", "Drivers");
        }

        /// <summary>
        /// ORDER TAKEN BY DRIVER
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Orders/Edit/5
        
        public async Task<ActionResult> AddDriver(int? id, Order order)
        {
            if (id == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = " Driver Id not found"
                });
            }
            var currentorder = await _context.Orders.FindAsync(id);
            if (currentorder == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Order not found"
                });
            }

            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
            currentorder.DriverIdentity = user;

            if (ModelState.IsValid)
            {
                _context.Entry(currentorder).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("OnDelivery", "Drivers");
            }

            return RedirectToAction("OnDelivery", "Drivers");
        }

        /// <summary>
        /// ORDER DELIVERED
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
        public async Task<ActionResult> OrderDelivered(int? id, Order order)
        {
            if (id == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = " Driver Id not found"
                });
            }
            var currentorder = await _context.Orders.FindAsync(id);
            if (currentorder == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Order not found"
                });
            }

            var user = _userManager.FindByNameAsync(User.Identity.Name).Result.Id;
            currentorder.IsDelivered = true;

            if (ModelState.IsValid)
            {
                _context.Entry(currentorder).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Delivered", "Drivers");
            }

            return RedirectToAction("Delivered", "Drivers");
        }

        // GET: Drivers
        public async Task<IActionResult> Index()
        {
              return _context.Drivers != null ? 
                          View(await _context.Drivers.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Drivers'  is null.");
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Drivers == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // GET: Drivers/Create
        public IActionResult Create()
        {
            //ViewData[index: User.Identity.ToString()] = new SelectList(_context.Users, "Id", "Name");
            return View();
        }

        // POST: Drivers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DriverId,DriverIdentity,Name")] Driver driver)
        {
            if (ModelState.IsValid)
            { 
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Drivers == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound();
            }
            return View(driver);
        }

        // POST: Drivers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DriverId,DriverIdentity,Name,OnLine,OnDelivery")] Driver driver)
        {
            if (id != driver.DriverId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.DriverId))
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
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Drivers == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Drivers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Drivers'  is null.");
            }
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(int id)
        {
          return (_context.Drivers?.Any(e => e.DriverId == id)).GetValueOrDefault();
        }
    }
}
