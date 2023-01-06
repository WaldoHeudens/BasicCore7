using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using BasicCore7.Data;
using BasicCore7.Models;
using BasicCore7.Controllers;

namespace BasicCore7.Controllers
{
    public class LanguagesController : CultController
    {

        public LanguagesController(BasicCore7Context context, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<CultController> logger)
            : base(context, httpContextAccessor, logger)
        {
        }

        // GET: Languages
        public async Task<IActionResult> Index()
        {
              return View(await _context.Language.ToListAsync());
        }

        // GET: Languages/Details/5

        // GET: Languages/Create
        public IActionResult Create()
        {
            Language model = new Language { Cultures = "" };

            return View(model);
        }

        // POST: Languages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Cultures,IsShown")] Language language)
        {
            if (language.Cultures == null)
                language.Cultures = "";
            if (ModelState.IsValid)
            {
                _context.Add(language);
                await _context.SaveChangesAsync();
                Language.Initialize(_context);
                // zorg dat het systeem beschikt over een nieuwe lijst van gebruikte cultures
                try
                {
                    var localizationOptions = new RequestLocalizationOptions()
                        .AddSupportedCultures(Language.SupportedCultures)
                        .AddSupportedUICultures(Language.SupportedCultures)
                        .SetDefaultCulture("nl-BE");
                }
                catch
                {
                    _context.Remove(language);
                    await _context.SaveChangesAsync();
                    ViewData["ErrorMessage"] = "De code moet 2 kleine letters zijn, de culturelijst telkens 2 hoofdletters, gescheiden door een ';'";
                    return View(language);
                }
                return RedirectToAction(nameof(Index));
            }

            return View(language);
        }

        // GET: Languages/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Language == null)
            {
                return NotFound();
            }

            var language = await _context.Language.FindAsync(id);
            if (language.Cultures == null)
                language.Cultures = "";
            return View(language);
        }

        // POST: Languages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Cultures,IsShown")] Language language)
        {
            if (language.Cultures == null)
                language.Cultures = "";
            Language oldLanguage = _context.Language.FirstOrDefault(l => l.Id == language.Id);

            if (ModelState.IsValid)
            {
                 _context.Update(language);
                 await _context.SaveChangesAsync();

                Language.Initialize(_context);
                // zorg dat het systeem beschikt over een nieuwe lijst van gebruikte cultures
                try
                {
                    var localizationOptions = new RequestLocalizationOptions()
                        .AddSupportedCultures(Language.SupportedCultures)
                        .AddSupportedUICultures(Language.SupportedCultures)
                        .SetDefaultCulture("nl-BE");
                }
                catch
                {
                    
                    _context.Update(oldLanguage);
                    await _context.SaveChangesAsync();
                    ViewData["ErrorMessage"] = "De code moet 2 kleine letters zijn, de culturelijst telkens 2 hoofdletters, gescheiden door een ';'";
                    return View(language);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(language);
        }

        public IActionResult ChangeLanguage(string id, string returnUrl)
        {
            string culture = Thread.CurrentThread.CurrentCulture.ToString();
            try
            {
                culture = id + culture.Substring(2, 3);
            }
            catch
            {
                culture = id + "-BE";
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            if (User.Identity.IsAuthenticated)
            {
                Language language = _context.Language.FirstOrDefault(l => l.Id == id);
                _user.Language = language;
                BasicCore7User user = _context.Users.FirstOrDefault(u => u.Id == _user.Id);
                user.Language = language;
                user.LanguageId = id;
                _context.SaveChanges();
            }


            return LocalRedirect(returnUrl);
        }
    }
}
