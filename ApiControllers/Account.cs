using Microsoft.AspNetCore.Mvc;
using GroupSpace2022.APIModels;
using Microsoft.AspNetCore.Identity;
using BasicCore7.Data;
using System.Security.Claims;

namespace GroupSpace2022.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly BasicCore7Context _context;
        private readonly SignInManager<BasicCore7User> _signInManager;

        public AccountController(BasicCore7Context context, SignInManager<BasicCore7User> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        // Dit is een eenvoudige login-methode gebruikmakend van de bestaande Identity-structuur
        // Hiervoor hebben we nodig:  Een login-model (deze zit in APIModels)


        // POST: api/Groups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("/API/Login")]
        public async Task<ActionResult<Boolean>> Login([FromBody]LoginModel @login)
        {
            var result = await _signInManager.PasswordSignInAsync(@login.UserName, @login.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return User.Identity.IsAuthenticated;
            }
            return false;
        }

    }
}
