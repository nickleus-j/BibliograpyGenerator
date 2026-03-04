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

            // Split by @ symbol to isolate individual entries
            var bibEntries = bibTexString.Split('@', StringSplitOptions.RemoveEmptyEntries);

            foreach (var bibEntry in bibEntries)
            {
                try
                {
                    var entry = ParseSingleBibEntry(bibEntry);
                    if (entry != null)
                        entries.Add(entry);
                }
                catch (Exception ex)
                {
                    // Log or handle parsing errors as needed
                    System.Diagnostics.Debug.WriteLine($"Error parsing BibTeX entry: {ex.Message}");
                }
            }

            return entries;
        }

        private static BibliographyEntry? ParseSingleBibEntry(string bibEntry)
        {
            // Extract entry type and key
            var typeAndKeyMatch = System.Text.RegularExpressions.Regex.Match(
                bibEntry, 
                @"^(\w+)\s*\{\s*([^,]+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (!typeAndKeyMatch.Success)
                return null;

            var entryType = typeAndKeyMatch.Groups[1].Value.ToLower();
            var entryKey = typeAndKeyMatch.Groups[2].Value.Trim();

            // Extract all key-value pairs
            var fields = ExtractBibFields(bibEntry);

            // Create the bibliography entry
            var entry = new BibliographyEntry
            {
                SourceType = MapSourceType(entryType),
                Title = ExtractFieldValue(fields, "title") ?? string.Empty,
                Publisher = ExtractFieldValue(fields, "publisher"),
                DigitalObjectIdentifier = ExtractFieldValue(fields, "doi"),
                Url = ExtractFieldValue(fields, "url"),
                ContainerTitle = ExtractFieldValue(fields, "journal") ?? ExtractFieldValue(fields, "booktitle"),
                Volume = ExtractFieldValue(fields, "volume"),
                Issue = ExtractFieldValue(fields, "number"),
                Pages = ExtractFieldValue(fields, "pages"),
            };

            // Parse publication date
            var yearStr = ExtractFieldValue(fields, "year");
            var monthStr = ExtractFieldValue(fields, "month");
            if (!string.IsNullOrEmpty(yearStr) && int.TryParse(yearStr, out var year))
            {
                entry.PublicationDate = new PublicationDate { Year = year };
                
                if (!string.IsNullOrEmpty(monthStr) && int.TryParse(monthStr, out var month))
                    entry.PublicationDate.Month = month;
            }

            // Parse access date (for websites)
            var accessDateStr = ExtractFieldValue(fields, "accessdate");
            if (!string.IsNullOrEmpty(accessDateStr) && DateOnly.TryParse(accessDateStr, out var accessDate))
                entry.AccessDate = accessDate;

            // Parse contributors (authors, editors, etc.)
            ParseContributors(fields, entry);

            return entry;
        }

        private static Dictionary<string, string> ExtractBibFields(string bibEntry)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            // Remove the entry type and key prefix
            var contentStart = bibEntry.IndexOf('{');
            var contentEnd = bibEntry.LastIndexOf('}');
            
            if (contentStart < 0 || contentEnd < 0 || contentStart >= contentEnd)
                return fields;

            var content = bibEntry.Substring(contentStart + 1, contentEnd - contentStart - 1);

            // Split by commas, but be careful with nested braces
            var fieldStrings = SplitByCommaRespectingBraces(content);

            foreach (var fieldString in fieldStrings)
            {
                var parts = fieldString.Split(new[] { '=' }, 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = CleanBibValue(parts[1].Trim());
                    fields[key] = value;
                }
            }

            return fields;
        }

        private static List<string> SplitByCommaRespectingBraces(string content)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            var braceDepth = 0;

            foreach (var ch in content)
            {
                if (ch == '{')
                    braceDepth++;
                else if (ch == '}')
                    braceDepth--;
                else if (ch == ',' && braceDepth == 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
        }

        private static string CleanBibValue(string value)
        {
            // Remove surrounding braces or quotes
            value = value.Trim();
            
            if ((value.StartsWith("{") && value.EndsWith("}")) ||
                (value.StartsWith("\"") && value.EndsWith("\"")))
            {
                value = value.Substring(1, value.Length - 2);
            }

            // Remove LaTeX commands (basic cleanup)
            value = System.Text.RegularExpressions.Regex.Replace(value, @"\\[a-z]+\{([^}]*)\}", "$1");
            value = System.Text.RegularExpressions.Regex.Replace(value, @"~", " ");

            return value.Trim();
        }

        private static string? ExtractFieldValue(Dictionary<string, string> fields, string fieldName)
        {
            return fields.TryGetValue(fieldName, out var value) && !string.IsNullOrEmpty(value) ? value : null;
        }

        private static SourceType MapSourceType(string bibEntryType)
        {
            return bibEntryType switch
            {
                "book" => SourceType.Book,
                "article" => SourceType.Journal,
                "website" or "misc" => SourceType.Website,
                "report" or "techreport" => SourceType.Report,
                _ => SourceType.Book // Default fallback
            };
        }

        private static void ParseContributors(Dictionary<string, string> fields, BibliographyEntry entry)
        {
            // Parse authors
            var authorsStr = ExtractFieldValue(fields, "author");
            if (!string.IsNullOrEmpty(authorsStr))
            {
                var authors = ParseAuthorString(authorsStr);
                foreach (var author in authors)
                {
                    author.Role = ContributorRole.Author;
                    entry.Contributors.Add(author);
                }
            }

            // Parse editors
            var editorsStr = ExtractFieldValue(fields, "editor");
            if (!string.IsNullOrEmpty(editorsStr))
            {
                var editors = ParseAuthorString(editorsStr);
                foreach (var editor in editors)
                {
                    editor.Role = ContributorRole.Editor;
                    entry.Contributors.Add(editor);
                }
            }
        }

        private static List<Contributor> ParseAuthorString(string authorString)
        {
            var contributors = new List<Contributor>();
            
            // Split by " and " (BibTeX standard)
            var authorParts = authorString.Split(new[] { " and " }, StringSplitOptions.TrimEntries);

            foreach (var authorPart in authorParts)
            {
                var names = authorPart.Split(',');
                
                if (names.Length == 2)
                {
                    // Format: "LastName, FirstName"
                    contributors.Add(new Contributor
                    {
                        LastName = names[0].Trim(),
                        FirstName = names[1].Trim()
                    });
                }
                else
                {
                    // Format: "FirstName LastName" - simple split on last space
                    var trimmed = authorPart.Trim();
                    var lastSpace = trimmed.LastIndexOf(' ');
                    
                    if (lastSpace > 0)
                    {
                        contributors.Add(new Contributor
                        {
                            FirstName = trimmed.Substring(0, lastSpace).Trim(),
                            LastName = trimmed.Substring(lastSpace + 1).Trim()
                        });
                    }
                    else
                    {
                        contributors.Add(new Contributor { LastName = trimmed });
                    }
                }
            }

            return contributors;
        }
    }
}
