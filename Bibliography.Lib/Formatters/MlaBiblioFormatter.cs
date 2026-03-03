using System.Text;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public class MlaBiblioFormatter:IBibliographyStyleFormatter
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
            string formattedEntry = FormatMla(entry);
            sb.AppendLine(formattedEntry);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
    private string FormatMla(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();
        var authorString = FormatAuthors(authors);
        return $"{authorString}. {entry.Title}. {entry.Publisher}, {entry.PublicationDate?.Year}.";
    }
    private string FormatAuthors(List<Contributor> authors)
    {
        if (authors.Count == 1)
            return $"{authors[0].LastName}, {authors[0].FirstName}.";

        if (authors.Count == 2)
            return $"{authors[0].LastName}, {authors[0].FirstName}, and {authors[1].FirstName} {authors[1].LastName}.";

        return $"{authors[0].LastName}, {authors[0].FirstName}, et al.";
    }
}