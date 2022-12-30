using BasicCore7.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BasicCore7.Models
{
    // Used for database parameters connection
    public class Global
    {
        [Display (Name = "Code")]
        public string Id { get; set; }

        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
