using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BasicCore7.Models;
using BasicCore7.Services;

namespace BasicCore7.Data;

public class BasicCore7Context : IdentityDbContext<BasicCore7User>
{
    public BasicCore7Context(DbContextOptions<BasicCore7Context> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<Global> Globals { get; set; }

    public DbSet<Language> Language { get; set; }
}
