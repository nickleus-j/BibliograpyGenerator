using System.Text;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public class ApaBiblioFormatter:IBibliographyStyleFormatter
{
    public string FormatBibliography(IEnumerable<BibliographyEntry> entries)
    {
        var sb = new StringBuilder();
        var sortedEntries = entries
            .OrderBy(e => e.Contributors.FirstOrDefault(c => c.Role == ContributorRole.Author)?.LastName ?? e.Title)
            .ToList();
        for(int i=0;i< sortedEntries.Count();i++)
        {
            var entry = sortedEntries.ElementAt(i);
            string formattedEntry = FormatApa(entry);
            sb.AppendLine(formattedEntry);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
    private string FormatApa(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();
        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year.ToString() ?? "n.d.";
        return $"{authorString} ({year}). {entry.Title}. {entry.Publisher}.";
    }
    private string FormatAuthors(List<Contributor> authors)
    {
        if (authors.Count == 0) return "Unknown Author";
        if (authors.Count == 1)
            return $"{authors[0].LastName}, {authors[0].FirstName?[0]}.";

        if (authors.Count == 2)
            return $"{authors[0].LastName}, {authors[0].FirstName?[0]}., & {authors[1].LastName}, {authors[1].FirstName?[0]}.";

        return $"{authors[0].LastName}, {authors[0].FirstName?[0]}., et al.";
    }
}