using Bibliography.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Bibliography.Lib.Formatters
{
    public class BibTexFormatter
    {
        private static BibTexFormatter Instance { get; set; }

        private BibTexFormatter() { }

        public static BibTexFormatter GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BibTexFormatter();
            }
            return Instance;
        }

        private string SanitizeKey(string title)
        {
            if (string.IsNullOrEmpty(title))
                return "unknown";

            // Remove spaces and special chars for BibTeX key
            var invalidChars = new[] { ' ', '{', '}', ':', ';', ',', '.', '-', '_', '?', '!' };
            var sanitized = title;
            foreach (var ch in invalidChars)
                sanitized = sanitized.Replace(ch.ToString(), "");

            return sanitized.Length > 10 ? sanitized.Substring(0, 10) : sanitized;
        }

        public string ToBibTeX(IEnumerable<BibliographyEntry> entries)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            var sb = new StringBuilder();
            var entryList = entries.ToList();

            for (int i = 0; i < entryList.Count; i++)
            {
                AppendBibTeXEntry(entryList[i], sb);

                // Add blank line between entries, but not after the last one
                if (i < entryList.Count - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private void AppendBibTeXEntry(BibliographyEntry entry, StringBuilder sb)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Generate citation key
            var firstContributor = entry.Contributors.FirstOrDefault();
            string lastName = firstContributor?.LastName ?? "anon";
            string year = entry.PublicationDate?.Year.ToString() ?? DateTime.Now.Year.ToString();
            string key = $"{lastName}{year}";

            // Determine entry type
            string entryType = entry.SourceType switch
            {
                SourceType.Book => "book",
                SourceType.Journal => "article",
                SourceType.Website => "misc",
                SourceType.Report => "techreport",
                _ => "misc"
            };

            // Start entry
            sb.AppendLine($"@{entryType}{{{key},");

            // Authors
            if (entry.Contributors.Any())
            {
                string authors = string.Join(" and ",
                    entry.Contributors.Select(c => FormatAuthorName(c)));
                sb.AppendLine($"    author = {{{authors}}},");
            }

            // Title
            sb.AppendLine($"    title = {{{entry.Title}}},");

            // Source-specific fields
            switch (entry.SourceType)
            {
                case SourceType.Book:
                    if (!string.IsNullOrEmpty(entry.Publisher))
                        sb.AppendLine($"    publisher = {{{entry.Publisher}}},");
                    break;

                case SourceType.Journal:
                    if (!string.IsNullOrEmpty(entry.ContainerTitle))
                        sb.AppendLine($"    journal = {{{entry.ContainerTitle}}},");
                    if (!string.IsNullOrEmpty(entry.Volume))
                        sb.AppendLine($"    volume = {{{entry.Volume}}},");
                    if (!string.IsNullOrEmpty(entry.Issue))
                        sb.AppendLine($"    number = {{{entry.Issue}}},");
                    if (!string.IsNullOrEmpty(entry.Pages))
                        sb.AppendLine($"    pages = {{{entry.Pages}}},");
                    break;

                case SourceType.Website:
                    if (!string.IsNullOrEmpty(entry.Url))
                        sb.AppendLine($"    url = {{{entry.Url}}},");
                    if (entry.AccessDate.HasValue)
                        sb.AppendLine($"    note = {{Accessed: {entry.AccessDate.Value:yyyy-MM-dd}}},");
                    break;

                case SourceType.Report:
                    if (!string.IsNullOrEmpty(entry.Publisher))
                        sb.AppendLine($"    institution = {{{entry.Publisher}}},");
                    break;
            }

            // Year (after source-specific fields)
            if (entry.PublicationDate != null && entry.PublicationDate.Year > 0)
            {
                sb.AppendLine($"    year = {{{entry.PublicationDate.Year}}},");
            }

            // DOI or URL
            if (!string.IsNullOrEmpty(entry.DigitalObjectIdentifier))
                sb.AppendLine($"    doi = {{{entry.DigitalObjectIdentifier}}}");
            else if (!string.IsNullOrEmpty(entry.Url) && entry.SourceType != SourceType.Website)
                sb.AppendLine($"    url = {{{entry.Url}}}");
            else
            {
                // Remove trailing comma from previous line if no DOI/URL
                if (sb.Length > 0 && sb[sb.Length - 2] == ',')
                {
                    sb.Length -= 2; // Remove ",\r\n"
                    sb.AppendLine();
                }
            }

            sb.AppendLine("}");
        }

        private string FormatAuthorName(Contributor contributor)
        {
            // Format: "FirstName LastName" (not "LastName, FirstName")
            if (string.IsNullOrEmpty(contributor.FirstName))
                return contributor.LastName;

            return $"{contributor.FirstName} {contributor.LastName}";
        }
    }

}
