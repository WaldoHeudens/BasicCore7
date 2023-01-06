// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using BasicCore7.Data;
using BasicCore7.Models;
using BasicCore7.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace BasicCore7.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<BasicCore7User> _userManager;
        private readonly SignInManager<BasicCore7User> _signInManager;
        private readonly IStringLocalizer<IndexModel> _localizer;
        private readonly BasicCore7Context _context;

        public IndexModel(
            UserManager<BasicCore7User> userManager,
            SignInManager<BasicCore7User> signInManager,
            IStringLocalizer<IndexModel> localizer,
            BasicCore7Context context)
        { 
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Language")]
            public string LanguageId { get; set; }
        }

        private async Task LoadAsync(BasicCore7User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LanguageId = user.LanguageId
            };
            ViewData["Languages"] = Language.Languages.Where(l => l.IsShown).ToList();
            ViewData["LanguageId"] = user.LanguageId;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(_localizer["Unable to load user with ID"] + $" '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = _localizer["Unexpected error when trying to set phone number."];
                    return RedirectToPage();
                }
            }

            BasicCore7User _user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            bool userChanged = false;
            if (Input.FirstName != _user.FirstName)
            {
                _user.FirstName = Input.FirstName;
                userChanged = true;
            }

            if (Input.LastName != _user.LastName)
            {
                _user.LastName = Input.LastName;
                userChanged = true;
            }

            if (Input.LanguageId != _user.LanguageId) 
            {
                _user.LanguageId = Input.LanguageId;
                userChanged = true;
                string culture = Thread.CurrentThread.CurrentCulture.ToString();
                try
                {
                    culture = Input.LanguageId + culture.Substring(2, 3);
                }
                catch
                {
                    culture = Input.LanguageId + "-BE";
                }

                // Added to make sure that the statusmessage would be in the new language
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

                // Added to make sure that the new culture is communicated by the cookies
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            }

            if (userChanged)
            {
                _context.Update(_user);
                _context.SaveChanges();
                Globals.ReloadUser(_user.UserName, _context);
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["Your profile has been updated"];
            return RedirectToPage();
        }
    }
}
