using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bibliography.Lib.Models
{
    public class Contributor
    {
        public string? FirstName { get; set; }

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public ContributorRole Role { get; set; }
    }

}
