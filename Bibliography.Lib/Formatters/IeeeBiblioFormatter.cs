using System.Text;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public class IeeeBiblioFormatter:IBibliographyStyleFormatter
{
    public string FormatBibliography(IEnumerable<BibliographyEntry> entries)
    {
        if (entries == null) throw new ArgumentNullException(nameof(entries));
        if (!entries.Any()) return "No entries provided.";
        var sb = new StringBuilder();

        for(int i=0;i< entries.Count();i++)
        {
            var entry = entries.ElementAt(i);
            string formattedEntry = FormatBibliographyEntry(entry,i+1);
            sb.AppendLine(formattedEntry);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
    private string FormatBibliographyEntry(BibliographyEntry entry,int index)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();
        var authorString = FormatAuthors(authors);
        return $"[{index}] {authorString}, \"{entry.Title},\" {entry.Publisher}, {entry.PublicationDate?.Year}.";
    }
    private string FormatAuthors(List<Contributor> authors)
    {
        if (authors.Count == 0) return "Unknown Author";
        if (authors.Count >= 7)
        {
            return $"{GetIeeeName(authors[0])} et al.";
        }

        if (authors.Count == 1)
        {
            return GetIeeeName(authors[0]);
        }

        var formattedNames = authors.Select(GetIeeeName).ToList();
        var allExceptLast = string.Join(", ", formattedNames.Take(formattedNames.Count - 1));
        var lastAuthor = formattedNames.Last();

        return formattedNames.Count==2? $"{allExceptLast} and {lastAuthor}" : $"{allExceptLast}, and {lastAuthor}";
    }
    private string GetIeeeName(Contributor author)
    {
        if (string.IsNullOrWhiteSpace(author.FirstName))
            return author.LastName;

        return $"{author.FirstName[0]}. {author.LastName}";
    }
}