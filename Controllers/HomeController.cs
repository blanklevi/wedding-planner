using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private int? uid
        {
            get
            {
                return HttpContext.Session.GetInt32("UserId");
            }
        }

        private bool isLoggedIn
        {
            get
            {
                return uid != null;
            }
        }

        private WeddingPlannerContext dbContext;
        public HomeController(WeddingPlannerContext context)
        {
            dbContext = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if (isLoggedIn)
            {
                RedirectToAction("Dashboard");
            }
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "is taken");
                }
            }

            if (ModelState.IsValid == false)
            {
                return View("Index");
            }

            PasswordHasher<User> hasher = new PasswordHasher<User>();
            newUser.Password = hasher.HashPassword(newUser, newUser.Password);

            dbContext.Users.Add(newUser);
            dbContext.SaveChanges();

            HttpContext.Session.SetInt32("UserId", newUser.UserId);
            HttpContext.Session.SetString("FullName", newUser.FullName());
            return RedirectToAction("Dashboard");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser loginUser)
        {
            string genericErrMsg = "Invalid Email or Password";

            if (ModelState.IsValid == false)
            {
                return View("Index");
            }

            User dbUser = dbContext.Users.FirstOrDefault(user => user.Email == loginUser.LoginEmail);

            if (dbUser == null)
            {
                ModelState.AddModelError("LoginEmail", genericErrMsg);
                return View("Index");
            }

            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
            PasswordVerificationResult pwCompareResult = hasher.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.LoginPassword);

            if (pwCompareResult == 0)
            {
                ModelState.AddModelError("LoginEmail", genericErrMsg);
                return View("Index");
            }

            HttpContext.Session.SetInt32("UserId", dbUser.UserId);
            HttpContext.Session.SetString("FullName", dbUser.FullName());
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            List<Wedding> AllWeddings = dbContext.Weddings
            .Include(w => w.Users)
            .ThenInclude(r => r.User)
            .ToList();
            ViewBag.AllWeddings = AllWeddings;
            return View("Dashboard", AllWeddings);
        }

        [HttpGet("newwedding")]
        public IActionResult NewWedding()
        {
            return View();
        }

        [HttpPost("addwedding")]
        public IActionResult AddWedding(Wedding newWed)
        {
            if (ModelState.IsValid)
            {
                newWed.Creator = (int)HttpContext.Session.GetInt32("UserId");
                dbContext.Add(newWed);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard", newWed);
            }
            else
            {
                return View("NewWedding");
            }
        }

        [HttpGet("wedding/{wedId}")]
        public IActionResult WeddingPage(int wedId)
        {
            Wedding selectedWed = dbContext.Weddings
            .Include(w => w.Users)
            .ThenInclude(r => r.User)
            .FirstOrDefault(w => w.WeddingId == wedId);
            if (selectedWed == null)
            {
                return RedirectToAction("Dashboard");
            }

            return View("Wedding", selectedWed);
        }

        [HttpPost]
        [Route("{userId}/{IsRsvp}/{wedId}")]
        public IActionResult Rsvp(int userId, bool IsRsvp, int wedId)
        {
            Rsvp newRsvp = new Rsvp()
            {
                UserId = userId,
                WeddingId = wedId,
                IsRsvp = IsRsvp
            };

            Rsvp existingRsvp = dbContext.Rsvps.FirstOrDefault(r => r.WeddingId == wedId && r.UserId == userId);

            if (existingRsvp == null)
            {
                dbContext.Rsvps.Add(newRsvp);
                dbContext.SaveChanges();
            }
            else
            {
                if (existingRsvp.IsRsvp != IsRsvp)
                {
                    existingRsvp.IsRsvp = IsRsvp;
                    existingRsvp.UpdatedAt = DateTime.Now;
                    dbContext.Rsvps.Update(existingRsvp);
                    dbContext.SaveChanges();
                }
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [Route("unrsvp/{IsRsvp}/{userId}/{wedId}")]
        public IActionResult UnRsvp(bool IsRsvp, int userId, int wedId)
        {
            Rsvp existingRsvp = dbContext.Rsvps.FirstOrDefault(r => r.WeddingId == wedId && r.UserId == userId);

            if (existingRsvp.IsRsvp != IsRsvp)
            {
                existingRsvp.IsRsvp = IsRsvp;
                existingRsvp.UpdatedAt = DateTime.Now;
                dbContext.Rsvps.Update(existingRsvp);
                dbContext.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [Route("delete/{userId}/{wedId}")]
        public IActionResult DeleteWed(int userId, int wedId)
        {
            Wedding selectedWed = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == wedId);
            if (selectedWed == null)
            {
                return RedirectToAction("Dashboard");
            }
            dbContext.Weddings.Remove(selectedWed);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
