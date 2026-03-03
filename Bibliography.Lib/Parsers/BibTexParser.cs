using Bibliography.Lib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Bibliography.Lib.Parsers
{
    public class BibTexParser
    {
        public IList<BibliographyEntry> ParseBibTexEntries(string bibTexString)
        {
            var entries = new List<BibliographyEntry>();

            if (string.IsNullOrWhiteSpace(bibTexString))
                return entries;

            // Regex to match BibTeX entries: @type{key, fields}
            var entryPattern = @"@(\w+)\s*\{\s*([^,]+),\s*(.*?)\n\s*\}";
            var matches = Regex.Matches(bibTexString, entryPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                try
                {
                    var entry = new BibliographyEntry();
                    var sourceTypeStr = match.Groups[1].Value.Trim();
                    var fieldsStr = match.Groups[3].Value;

                    // Parse source type
                    if (!Enum.TryParse<SourceType>(sourceTypeStr, true, out var sourceType))
                        sourceType = SourceType.Book;
                    entry.SourceType = sourceType;

                    // Parse fields into a dictionary
                    var fields = ParseBibTexFields(fieldsStr);

                    // Map BibTeX fields to BibliographyEntry properties
                    if (fields.TryGetValue("title", out var title))
                        entry.Title = CleanBibTexValue(title);

                    if (fields.TryGetValue("publisher", out var publisher))
                        entry.Publisher = CleanBibTexValue(publisher);

                    if (fields.TryGetValue("doi", out var doi))
                        entry.DigitalObjectIdentifier = CleanBibTexValue(doi);

                    if (fields.TryGetValue("url", out var url))
                        entry.Url = CleanBibTexValue(url);

                    if (fields.TryGetValue("journal", out var journal))
                        entry.ContainerTitle = CleanBibTexValue(journal);

                    if (fields.TryGetValue("volume", out var volume))
                        entry.Volume = CleanBibTexValue(volume);

                    if (fields.TryGetValue("number", out var issue))
                        entry.Issue = CleanBibTexValue(issue);

                    if (fields.TryGetValue("pages", out var pages))
                        entry.Pages = CleanBibTexValue(pages);

                    // Parse authors/editors
                    ParseContributors(fields, entry);

                    // Parse publication date
                    if (fields.TryGetValue("year", out var year))
                    {
                        if (int.TryParse(CleanBibTexValue(year), out var yearInt))
                        {
                            entry.PublicationDate = new PublicationDate { Year = yearInt };

                            if (fields.TryGetValue("month", out var month))
                            {
                                var monthValue = CleanBibTexValue(month);
                                if (int.TryParse(monthValue, out var monthInt))
                                    entry.PublicationDate.Month = monthInt;
                            }
                        }
                    }

                    // Parse access date for websites
                    if (fields.TryGetValue("urldate", out var accessDate))
                    {
                        if (DateOnly.TryParse(CleanBibTexValue(accessDate), out var accessDateOnly))
                            entry.AccessDate = accessDateOnly;
                    }

                    entries.Add(entry);
                }
                catch
                {
                    // Log or handle parsing errors as needed
                    continue;
                }
            }

            return entries;
        }

        private Dictionary<string, string> ParseBibTexFields(string fieldsStr)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var fieldPattern = @"(\w+)\s*=\s*([{""'].*?[}""']|[^,}]+)";
            var matches = Regex.Matches(fieldsStr, fieldPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var fieldName = match.Groups[1].Value.Trim();
                var fieldValue = match.Groups[2].Value.Trim();
                fields[fieldName] = fieldValue;
            }

            return fields;
        }

        private string CleanBibTexValue(string value)
        {
            // Remove surrounding braces or quotes
            value = Regex.Replace(value, @"^[{""']|[}""']$", "").Trim();
            // Remove inner braces used for case protection
            value = Regex.Replace(value, @"\{|\}", "");
            return value;
        }

        private void ParseContributors(Dictionary<string, string> fields, BibliographyEntry entry)
        {
            var authorPattern = @"(\w+)\s+(\w+)";

            if (fields.TryGetValue("author", out var authorsStr))
            {
                var authors = authorsStr.Split(new[] { " and " }, StringSplitOptions.None);
                foreach (var author in authors)
                {
                    var match = Regex.Match(author.Trim(), authorPattern);
                    if (match.Success)
                    {
                        entry.Contributors.Add(new Contributor
                        {
                            FirstName = match.Groups[1].Value,
                            LastName = match.Groups[2].Value,
                            Role = ContributorRole.Author
                        });
                    }
                }
            }

            if (fields.TryGetValue("editor", out var editorsStr))
            {
                var editors = editorsStr.Split(new[] { " and " }, StringSplitOptions.None);
                foreach (var editor in editors)
                {
                    var match = Regex.Match(editor.Trim(), authorPattern);
                    if (match.Success)
                    {
                        entry.Contributors.Add(new Contributor
                        {
                            FirstName = match.Groups[1].Value,
                            LastName = match.Groups[2].Value,
                            Role = ContributorRole.Editor
                        });
                    }
                }
            }
        }

    }
}
