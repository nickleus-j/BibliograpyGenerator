using System.Text;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public class ChicagoBiblioFormatter:IBibliographyStyleFormatter
{
    public string FormatBibliography(IEnumerable<BibliographyEntry> entries)
    {
        if (entries == null) throw new ArgumentNullException(nameof(entries));
        if (!entries.Any()) return "No entries provided.";
        var sortedEntries = entries
            .OrderBy(e => e.Contributors.FirstOrDefault(c => c.Role == ContributorRole.Author)?.LastName ?? e.Title)
            .ToList();
        var sb = new StringBuilder();

        for(int i=0;i< sortedEntries.Count();i++)
        {
            var entry = sortedEntries.ElementAt(i);
            string formattedEntry = FormatBibliographyEntry(entry);
            sb.AppendLine(formattedEntry);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
    private string FormatBibliographyEntry(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();
        var authorString = FormatAuthors(authors);
        return $"{authorString} {entry.Title}. {entry.Publisher}, {entry.PublicationDate?.Year}.";
    }
    private string FormatAuthors(List<Contributor> authors)
    {
        if (authors.Count == 0) return "Unknown Author";
        if (authors.Count >= 7)
        {
            var firstSeven = authors.Take(7).Select((a, i) => i == 0 ? $"{a.LastName}, {a.FirstName}" : $"{a.FirstName} {a.LastName}");
            return string.Join(", ", firstSeven) + ", et al.";
        }

        if (authors.Count == 1)
            return $"{authors[0].LastName}, {authors[0].FirstName}.";

        var result = new List<string> { $"{authors[0].LastName}, {authors[0].FirstName}" };
        for (int i = 1; i < authors.Count - 1; i++)
        {
            result.Add($"{authors[i].FirstName} {authors[i].LastName}");
        }

        return string.Join(", ", result) + ", and " + $"{authors.Last().FirstName} {authors.Last().LastName}.";
    }
}