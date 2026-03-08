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
        return entry.SourceType switch
        {
            SourceType.Book => FormatBook(entry),
            SourceType.Journal => FormatJournal(entry),
            SourceType.Website => FormatWebpage(entry),
            SourceType.Report => FormatTechnicalReport(entry),
            _ => throw new ArgumentException($"Unsupported source type: {entry.SourceType}")
        };
    }

    private string FormatBook(BibliographyEntry entry)
    {
        var authorString = FormatAuthors(GetAuthors(entry));
        return $"{authorString}. {entry.Title}. {entry.Publisher}, {entry.PublicationDate?.Year}.";
    }

    private string FormatJournal(BibliographyEntry entry)
    {
        var authorString = FormatAuthors(GetAuthors(entry));
        var year = entry.PublicationDate?.Year;
        var volume = entry.Volume ?? string.Empty;
        var issue = entry.Issue ?? string.Empty;
        var pages = entry.Pages ?? string.Empty;

        // MLA format: Author(s). "Article Title." Journal Title, vol. #, no. #, Year, pp. page range.
        var issueInfo = !string.IsNullOrEmpty(issue) ? $", no. {issue}" : string.Empty;
        var pageInfo = !string.IsNullOrEmpty(pages) ? $", pp. {pages}" : string.Empty;

        return $"{authorString}. \"{entry.Title}.\" {entry.Publisher}, vol. {volume}{issueInfo}, {year}{pageInfo}.";
    }

    private string FormatWebpage(BibliographyEntry entry)
    {
        var authorString = FormatAuthors(GetAuthors(entry));
        var year = entry.PublicationDate?.Year.ToString() ?? "n.d.";
        var url = entry.Url ?? string.Empty;
        var accessDate = entry.AccessDate?.ToString("d MMM. yyyy") ?? string.Empty;

        // MLA format: Author(s). "Page Title." Website Name, Year, URL. Accessed date.
        var accessInfo = !string.IsNullOrEmpty(accessDate) ? $" Accessed {accessDate}." : string.Empty;

        return $"{authorString}. \"{entry.Title}.\" {entry.Publisher}, {year}, {url}.{accessInfo}";
    }

    private string FormatTechnicalReport(BibliographyEntry entry)
    {
        var authorString = FormatAuthors(GetAuthors(entry));
        var year = entry.PublicationDate?.Year;
        var reportNumber = entry.Issue ?? string.Empty;
        var organization = entry.Publisher ?? entry.Publisher ?? string.Empty;

        // MLA format: Author(s). Title of Report. Report Number, Organization, Year.
        var reportInfo = !string.IsNullOrEmpty(reportNumber) ? $"{reportNumber}, " : string.Empty;

        return $"{authorString}. {entry.Title}. {reportInfo}{organization}, {year}.";
    }
    private List<Contributor> GetAuthors(BibliographyEntry entry)
    {
        return entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();
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