// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using BasicCore7.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using BasicCore7.Localizing;


namespace BasicCore7.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<BasicCore7User> _signInManager;
        private readonly UserManager<BasicCore7User> _userManager;
        private readonly IUserStore<BasicCore7User> _userStore;
        private readonly IUserEmailStore<BasicCore7User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly BasicCore7Context _context;
        private readonly IStringLocalizer<RegisterModel> _localizer;

        public RegisterModel(
            UserManager<BasicCore7User> userManager,
            IUserStore<BasicCore7User> userStore,
            SignInManager<BasicCore7User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            BasicCore7Context context, 
            IStringLocalizer<RegisterModel> localizer)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _localizer = localizer;
        }

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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Display(Name = "User Name")]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Required")]
            public string UserName { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Required")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Required")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Required")]
            [EmailAddress (ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "EmailAddress")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "StringLength", MinimumLength = 6)]
            [DataType(DataType.Password, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Password")]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Compare")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                BasicCore7User tempUser = await _emailStore.FindByEmailAsync(Input.Email, CancellationToken.None);
                if (tempUser != null) 
                {
                    ModelState.AddModelError(string.Empty, _localizer["A user with this e-mail address already exists."]);
                    return Page();
                }
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.LanguageId = Thread.CurrentThread.CurrentCulture.ToString().Substring(0, 2);
                user.Language = _context.Language.FirstOrDefault(l => l.Id == user.LanguageId);
                user.AddedOn = DateTime.Now;
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    _userManager.AddToRoleAsync(user, "User");          // By purpose not awaited
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                    string emailtext = _localizer["Please confirm your account by"] + $" <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> *1! </a>.";
                    emailtext = emailtext.Replace("*1!", _localizer["clicking here"]);
                    await _emailSender.SendEmailAsync(Input.Email, _localizer["Confirm your email"], emailtext);
                    return LocalRedirect(returnUrl);

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    //}
                    //else
                    //{
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return LocalRedirect(returnUrl);
                    //}
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, _localizer[error.Code]);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private BasicCore7User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<BasicCore7User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(BasicCore7User)}'. " +
                    $"Ensure that '{nameof(BasicCore7User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<BasicCore7User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<BasicCore7User>)_userStore;
        }
    }
}
