using BasicCore7.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Infrastructure.Internal;
using BasicCore7.Models;
using BasicCore7.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("CultTestDB");
builder.Services.AddDbContext<BasicCore7.Data.BasicCore7Context>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<BasicCore7User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BasicCore7.Data.BasicCore7Context>();
builder.Services.AddControllersWithViews();


builder.Services.AddLocalization(option => option.ResourcesPath = "Localizing");
    //.AddRazorPages();
builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IEmailSender, MailKitEmailSender>();
//builder.Services.Configure<MailKitOptions>(options =>
//{
//    options.Server = builder.Configuration["ExternalProviders:MailKit:SMTP:Address"];
//    options.Port = Convert.ToInt32(builder.Configuration["ExternalProviders:MailKit:SMTP:Port"]);
//    options.Account = builder.Configuration["ExternalProviders:MailKit:SMTP:Account"];
//    options.Password = builder.Configuration["ExternalProviders:MailKit:SMTP:Password"];
//    options.SenderEmail = builder.Configuration["ExternalProviders:MailKit:SMTP:SenderEmail"];
//    options.SenderName = builder.Configuration["ExternalProviders:MailKit:SMTP:SenderName"];

//    // Set it to TRUE to enable ssl or tls, FALSE otherwise
//    options.Security = false;  // true zet ssl or tls aan
//});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

// Zorg ervoor dat TempData beschouwd wordt als essenti�le cookie, en dus altijd bestaat
builder.Services.Configure<CookieTempDataProviderOptions>(options => {
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//Initialize and update the database
// and make sure fundamental "Globals" have been set
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<BasicCore7User>>();
    await SeedDatacontext.Initialize(services, userManager);
}

// Add the list of used cultures
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("nl-BE")
    .AddSupportedCultures(Language.SupportedCultures)
    .AddSupportedUICultures(Language.SupportedCultures);

app.UseRequestLocalization(localizationOptions);


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Added manually as Identity Framework hasn't been written following MVC
app.MapRazorPages();

// Add the "Globals" middleware
app.UseMiddleware<Globals>();


app.Run();
