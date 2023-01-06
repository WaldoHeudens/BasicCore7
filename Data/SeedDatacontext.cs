using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BasicCore7.Models;
using BasicCore7.Services;
using Microsoft.IdentityModel.Tokens;

namespace BasicCore7.Data
{
    public class SeedDatacontext
    {
        public static async Task<IActionResult> Initialize(System.IServiceProvider serviceProvider, 
                    UserManager<BasicCore7User> userManager)
        {
            using (var context = new BasicCore7Context(serviceProvider.GetRequiredService<DbContextOptions<BasicCore7Context>>()))
            {
                //context.Database.EnsureCreated();   // Creates database, incl. tables, but doesn't add migration
                context.Database.Migrate();

                // Get the global parameters
                List<Global> parameters = context.Globals.ToList();

                string dbVersion = "";

                if (!parameters.IsNullOrEmpty()) 
                {
                    dbVersion = parameters.FirstOrDefault(p => p.Id == "Version").Value;
                }

                if (dbVersion == "")
                {
                    // Initialize global parameters
                    context.Globals.AddRange(
                        new Global { Id = "EmailAccount", Value = "???", Description = "Smtp Server Account Name"},
                        new Global { Id = "EmailEmail", Value = "support@BasicCore7.org", Description = "Smtp Server Sender Email Address" },
                        new Global { Id = "EmailPassword", Value = "???", Description = "Smtp Server Password" },
                        new Global { Id = "EmailPort", Value = "1025", Description = "Smtp Server Port" },
                        new Global { Id = "EmailSecurity", Value = "false", Description = "Smtp Server Enable ssl or tsl" },
                        new Global { Id = "EmailSender", Value = "Administrator@BasicCore7.org", Description = "Smtp Server Sender name" },
                        new Global { Id = "EmailServer", Value = "relay-auth.mailprotect.be", Description = "Email smtp server" },
                        new Global { Id = "Version", Value = "0.01", Description = "Database version" }
                    );
                

                    // Initialize the languages
                    context.Language.AddRange
                        (
                            new Language() { Id = "-", Name = "-", Cultures = "", IsShown = false },
                            new Language() { Id = "en", Name = "English", Cultures = "UK;US", IsShown = true },
                            new Language() { Id = "fr", Name = "français", Cultures = "BE;FR", IsShown = true },
                            new Language() { Id = "nl", Name = "Nederlands", Cultures = "BE;NL", IsShown = true }
                        );
                        context.SaveChanges();
                }

                // Load the global paramters
                Globals.ApplyGlobals(context);
                
                // Initialize the languages
                Language.Initialize(context);

                // Initialisatie van de users en de rollen

                BasicCore7User dummy = null;
                BasicCore7User user = null;
                BasicCore7User administrator = null;

                if (!context.Roles.Any())
                {

                    dummy = new BasicCore7User
                    {
                        Email = "?@?.?",
                        EmailConfirmed = true,
                        LockoutEnabled = true,
                        UserName = "dummy",
                        FirstName = "?",
                        LastName = "?",
                        AddedOn = DateTime.Now
                    };
                    administrator = new BasicCore7User
                    {
                        Email = "admin@BasicCore7.org",
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                        UserName = "Administrator",
                        FirstName = "Administrator",
                        LastName = "BasicCore7",
                        AddedOn = DateTime.Now
                    };
                    user = new BasicCore7User
                    {
                        Email = "contributor@BasicCore7.org",
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                        UserName = "Contributor",
                        FirstName = "Contributor",
                        LastName = "BasicCore7",
                        AddedOn = DateTime.Now
                    };

                    await userManager.CreateAsync(administrator, "Abc!12345");
                    await userManager.CreateAsync(dummy, "Abc!12345");
                    await userManager.CreateAsync(user, "Abc!12345");

                    context.Roles.AddRange
                    (
                        new IdentityRole { Id = "SystemAdministrator", Name = "SystemAdministrator", NormalizedName = "SYSTEMADMINISTRATOR" },
                        new IdentityRole { Id = "UserAdministrator", Name = "UserAdministrator", NormalizedName = "USERADMINISTRATOR" },
                        new IdentityRole { Id = "User", Name = "User", NormalizedName = "USER" }
                    );
                    context.SaveChanges();

                    string id = administrator.Id;

                    context.UserRoles.AddRange
                        (
                            new IdentityUserRole<string> { RoleId = "UserAdministrator", UserId = administrator.Id },
                            new IdentityUserRole<string> { RoleId = "SystemAdministrator", UserId = administrator.Id },
                            new IdentityUserRole<string> { RoleId = "User", UserId = user.Id }
                        );
                    context.SaveChanges();
                }

                //if (dummy == null)
                //{
                //    dummy = context.Users.FirstOrDefault(u => u.UserName == "dummy");
                //    administrator = context.Users.FirstOrDefault(u => u.UserName == "Administrator");
                //    contributor = context.Users.FirstOrDefault(u => u.UserName == "Contributor");
                //}


                return null;
            }
        }
    }
}
