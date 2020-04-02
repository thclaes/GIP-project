﻿using System.Diagnostics;
using System.Threading.Tasks;
using Gip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Gip.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private gipDatabaseContext db = new gipDatabaseContext();

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // GET /
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index()
        {
            ApplicationUser user = null;

            if (signInManager.IsSignedIn(User)) 
            {
                user = await userManager.FindByNameAsync(User.Identity.Name);
            }

            return View(user);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}