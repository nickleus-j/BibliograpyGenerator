using Bibliography.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bibliography.Lib.Formtters
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
            // Remove spaces and special chars for BibTeX key
            var invalidChars = new[] { ' ', '{', '}', ':', ';', ',', '.', '-', '_' };
            foreach (var ch in invalidChars) title = title.Replace(ch.ToString(), "");
            return title.Length > 10 ? title.Substring(0, 10) : title;
        }
        public string ToBibTeX(IEnumerable<BibliographyEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            var sb = new StringBuilder();

            foreach (var entry in entries)
            {
                AppendBibTeXEntry(entry,sb);
                sb.AppendLine(); // separate entries with a blank line
            }

            return sb.ToString().Trim();
        }

        private void AppendBibTeXEntry(BibliographyEntry entry, StringBuilder sb)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            var firstContributor = entry.Contributors.FirstOrDefault();
            string key = $"{firstContributor?.LastName ?? "anon"}{entry.PublicationDate?.Year ?? DateTime.Now.Year}{SanitizeKey(entry.Title)}";

            string entryType = entry.SourceType switch
            {
                SourceType.Book => "book",
                SourceType.Journal => "article",
                SourceType.Website => "misc",
                SourceType.Report => "techreport",
                _ => "misc"
            };

            sb.AppendLine($"@{entryType}{{{key},");

            // Authors
            if (entry.Contributors.Any())
            {
                string authors = string.Join(" and ",
                    entry.Contributors.Select(c => $"{c.LastName}, {c.FirstName}"));
                sb.AppendLine($"  author = {{{authors}}},");
            }

            sb.AppendLine($"  title = {{{entry.Title}}},");

            // Year/Date
            if (entry.PublicationDate != null)
            {
                sb.AppendLine($"  year = {{{entry.PublicationDate.Year}}},");
                if (entry.PublicationDate.Month.HasValue)
                    sb.AppendLine($"  month = {{{entry.PublicationDate.Month.Value}}},");
                if (entry.PublicationDate.Day.HasValue)
                    sb.AppendLine($"  day = {{{entry.PublicationDate.Day.Value}}},");
            }

            // Source-specific fields
            switch (entry.SourceType)
            {
                case SourceType.Book:
                    if (!string.IsNullOrEmpty(entry.Publisher))
                        sb.AppendLine($"  publisher = {{{entry.Publisher}}},");
                    break;

                case SourceType.Journal:
                    if (!string.IsNullOrEmpty(entry.ContainerTitle))
                        sb.AppendLine($"  journal = {{{entry.ContainerTitle}}},");
                    if (!string.IsNullOrEmpty(entry.Volume))
                        sb.AppendLine($"  volume = {{{entry.Volume}}},");
                    if (!string.IsNullOrEmpty(entry.Issue))
                        sb.AppendLine($"  number = {{{entry.Issue}}},");
                    if (!string.IsNullOrEmpty(entry.Pages))
                        sb.AppendLine($"  pages = {{{entry.Pages}}},");
                    break;

                case SourceType.Website:
                    if (!string.IsNullOrEmpty(entry.Url))
                        sb.AppendLine($"  howpublished = {{\\url{{{entry.Url}}}}},");
                    if (entry.AccessDate.HasValue)
                        sb.AppendLine($"  note = {{Accessed: {entry.AccessDate.Value:yyyy-MM-dd}}},");
                    break;

                case SourceType.Report:
                    if (!string.IsNullOrEmpty(entry.Publisher))
                        sb.AppendLine($"  institution = {{{entry.Publisher}}},");
                    break;
            }

            // DOI or URL
            if (!string.IsNullOrEmpty(entry.DigitalObjectIdentifier))
                sb.AppendLine($"  doi = {{{entry.DigitalObjectIdentifier}}},");
            else if (!string.IsNullOrEmpty(entry.Url) && entry.SourceType != SourceType.Website)
                sb.AppendLine($"  url = {{{entry.Url}}},");

            sb.AppendLine("}");
        }
    }
}
