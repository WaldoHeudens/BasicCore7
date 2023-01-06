using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BasicCore7.Models;
using Microsoft.AspNetCore.Identity;

namespace BasicCore7.Data;

// Add profile data for application users by adding properties to the BasicCore7User class
public class BasicCore7User : IdentityUser
{
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = "?";

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = "?";

    [Display(Name = "Blocked")]
    public DateTime Blocked { get; set; } = DateTime.MaxValue;


    [ForeignKey("Language")]
    public string LanguageId { get; set; } = "nl";
    public Language? Language { get; set; }

    public DateTime AddedOn { get; set; } = DateTime.Now;

}

