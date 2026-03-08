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
    private string FormatBibliographyEntry(BibliographyEntry entry, int index)
    {
        return entry.SourceType switch
        {
            SourceType.Book => FormatBook(entry, index),
            SourceType.Journal => FormatJournal(entry, index),
            SourceType.Website => FormatWebsite(entry, index),
            SourceType.Report => FormatReport(entry, index),
            _ => FormatDefault(entry, index)
        };
    }

    private string FormatBook(BibliographyEntry entry, int index)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        return $"[{index}] {authorString}, \"{entry.Title},\" {entry.Publisher}, {year}.";
    }

    private string FormatJournal(BibliographyEntry entry, int index)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        var result = $"[{index}] {authorString}, \"{entry.Title},\" {entry.ContainerTitle}";

        if (!string.IsNullOrEmpty(entry.Volume))
            result += $", vol. {entry.Volume}";

        if (!string.IsNullOrEmpty(entry.Issue))
            result += $", no. {entry.Issue}";

        if (!string.IsNullOrEmpty(entry.Pages))
            result += $", pp. {entry.Pages}";

        result += $", {year}.";

        if (!string.IsNullOrEmpty(entry.DigitalObjectIdentifier))
            result += $" doi: {entry.DigitalObjectIdentifier}";

        return result;
    }

    private string FormatWebsite(BibliographyEntry entry, int index)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = authors.Any() ? FormatAuthors(authors) : "Unknown Author";
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        var result = $"[{index}] {authorString}, \"{entry.Title},\"";

        if (!string.IsNullOrEmpty(entry.Url))
            result += $" [Online]. Available: {entry.Url}";

        result += $" [Accessed: {(entry.AccessDate.HasValue ? entry.AccessDate.Value.ToString("MMM. dd, yyyy") : "date unknown")}].";

        return result;
    }

    private string FormatReport(BibliographyEntry entry, int index)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        var result = $"[{index}] {authorString}, \"{entry.Title},\" Tech. Rep.";

        if (!string.IsNullOrEmpty(entry.Publisher))
            result += $" {entry.Publisher}";

        result += $", {year}.";

        if (!string.IsNullOrEmpty(entry.DigitalObjectIdentifier))
            result += $" doi: {entry.DigitalObjectIdentifier}";
        else if (!string.IsNullOrEmpty(entry.Url))
            result += $" [Online]. Available: {entry.Url}";

        return result;
    }

    private string FormatDefault(BibliographyEntry entry, int index)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        return $"[{index}] {authorString}, \"{entry.Title},\" {year}.";
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