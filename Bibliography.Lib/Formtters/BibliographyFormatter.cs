using Bibliography.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bibliography.Lib.Formtters
{
    public class BibliographyFormatter
    {
        private static BibliographyFormatter Instance { get; set; }
        private BibliographyFormatter() { }
        public static BibliographyFormatter GetInstance()
        {
            if(Instance == null)
            {
                Instance = new BibliographyFormatter();
            }
            return Instance;
        }
        public string FormatBibliography(IEnumerable<BibliographyEntry> entries, CitationStyle style)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            var sb = new StringBuilder();

            // Sort by author last name then year for consistency
            var sortedEntries = entries
                .OrderBy(e => e.Contributors.FirstOrDefault()?.LastName ?? "")
                .ThenBy(e => e.PublicationDate?.Year ?? int.MaxValue);

            foreach (var entry in sortedEntries)
            {
                // Override citation style for each entry
                entry.CitationStyle = style;

                string citation = FormatCitation(entry);
                sb.AppendLine(citation);
                sb.AppendLine(); // extra line between entries
            }

            return sb.ToString().Trim();
        }
        public string FormatCitation(BibliographyEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var contributors = string.Join(", ",
                entry.Contributors.Select(c =>
                    $"{c.LastName}, {c.FirstName} ({c.Role})"));

            string date = entry.PublicationDate != null
                ? $"{entry.PublicationDate.Year}" +
                  (entry.PublicationDate.Month.HasValue ? $"-{entry.PublicationDate.Month.Value:D2}" : "") +
                  (entry.PublicationDate.Day.HasValue ? $"-{entry.PublicationDate.Day.Value:D2}" : "")
                : "n.d.";

            switch (entry.CitationStyle)
            {
                case CitationStyle.APA:
                    return $"{contributors} ({date}). {entry.Title}. {entry.Publisher ?? entry.ContainerTitle ?? ""}{(entry.DigitalObjectIdentifier != null ? $" doi:{entry.DigitalObjectIdentifier}" : "")}{(entry.Url != null ? $" Retrieved from {entry.Url}" : "")}";

                case CitationStyle.MLA:
                    return $"{contributors}. \"{entry.Title}.\" {(entry.ContainerTitle ?? entry.Publisher ?? "")}, {date}.{(entry.Pages != null ? $" pp. {entry.Pages}" : "")}{(entry.Url != null ? $" {entry.Url}" : "")}";

                case CitationStyle.Chicago:
                    return $"{contributors}. {entry.Title}. {(entry.Publisher ?? entry.ContainerTitle ?? "")}, {date}.{(entry.DigitalObjectIdentifier != null ? $" doi:{entry.DigitalObjectIdentifier}" : "")}{(entry.Url != null ? $" {entry.Url}" : "")}";

                case CitationStyle.Harvard:
                    return $"{contributors} ({date}) {entry.Title}. {(entry.Publisher ?? entry.ContainerTitle ?? "")}.{(entry.Url != null ? $" Available at: {entry.Url}" : "")}{(entry.AccessDate.HasValue ? $" (Accessed: {entry.AccessDate.Value:yyyy-MM-dd})" : "")}";

                case CitationStyle.IEEE:
                    return $"{contributors}, \"{entry.Title},\" {(entry.ContainerTitle ?? entry.Publisher ?? "")}, {date}.{(entry.DigitalObjectIdentifier != null ? $" doi:{entry.DigitalObjectIdentifier}" : "")}{(entry.Url != null ? $" {entry.Url}" : "")}";

                default:
                    return $"{contributors}. {entry.Title}. {date}.";
            }
        }
    }
}
