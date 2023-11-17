using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Threading.Tasks;
using _2faBackend.Models;

namespace _2faBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            // Stuur de gebruiker door naar de externe Google OAuth 2.0-provider voor autorisatie
            var authenticationProperties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, returnUrl);
            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-oauth")]
        [AllowAnonymous]
        public async Task<IActionResult> SignInOAuthCallback(string returnUrl = "/")
        {
            // Haal de externe inloggegevens op van de externe OAuth 2.0-provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Fout afhandelen indien externe inloggegevens niet beschikbaar zijn
                return BadRequest("Failed to retrieve external login information.");
            }

            // Log de gebruiker in met de externe inloggegevens
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                // Als de gebruiker met succes is ingelogd, stuur door naar het opgegeven retour-URL
                return Redirect(returnUrl);
            }
            else
            {
                // Als de gebruiker niet is ingeschreven, kan je hier de gebruiker registreren of een foutmelding teruggeven
                return BadRequest("Failed to sign in.");
            }
        }

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Log de huidige gebruiker uit
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Ok("Logged out successfully.");
        }
    }
}
