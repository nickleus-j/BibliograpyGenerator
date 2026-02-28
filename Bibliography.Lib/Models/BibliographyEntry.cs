using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace Bibliography.Lib.Models
{
    public class BibliographyEntry
    {
        [Required]
        public CitationStyle CitationStyle { get; set; } = CitationStyle.APA;

        [Required]
        public SourceType SourceType { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty; 

        public List<Contributor> Contributors { get; set; } = new();

        public PublicationDate? PublicationDate { get; set; }

        public string? Publisher { get; set; }
        public string? DigitalObjectIdentifier { get; set; }
        public string? Url { get; set; }

        // Journal-specific
        public string? ContainerTitle { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }

        // Website-specific: DateOnly is cleaner for "format": "date"
        public DateOnly? AccessDate { get; set; }
    }

}
