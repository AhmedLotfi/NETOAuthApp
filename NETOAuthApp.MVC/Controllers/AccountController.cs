using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NETOAuthApp.MVC.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("~/login")]
        public IActionResult Login(string returnUrl = "/")
        {
            string? redirectUrl = Url.Action(nameof(LoginCallback), "Account", new { returnUrl });
            AuthenticationProperties properties = new () { RedirectUri = redirectUrl };
            return Challenge(properties, "GitHub");
        }

        [HttpGet("~/signin-github")]
        public async Task<IActionResult> LoginCallback()
        {
            AuthenticateResult authenticateResult = await HttpContext.AuthenticateAsync("GitHub");

            if (!authenticateResult.Succeeded)
                return BadRequest();

            IEnumerable<Claim> claims = authenticateResult.Principal.Claims;
            return View(claims);
        }

        [HttpGet("~/logout")]
        public IActionResult Logout()
        {
            return SignOut("GitHub", "Cookies");
        }
    }
}