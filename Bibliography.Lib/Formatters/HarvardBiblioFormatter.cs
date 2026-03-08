using System.Text;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public class HarvardBiblioFormatter:IBibliographyStyleFormatter
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
        return entry.SourceType switch
        {
            SourceType.Book => FormatBook(entry),
            SourceType.Journal => FormatJournal(entry),
            SourceType.Website => FormatWebsite(entry),
            SourceType.Report => FormatReport(entry),
            _ => FormatDefault(entry)
        };
    }

    private string FormatBook(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        return $"{authorString} ({year}) {entry.Title}. {entry.Publisher}.";
    }

    private string FormatJournal(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        var journalPart = $"{entry.ContainerTitle}";

        if (!string.IsNullOrEmpty(entry.Volume))
            journalPart += $", {entry.Volume}";

        if (!string.IsNullOrEmpty(entry.Issue))
            journalPart += $"({entry.Issue})";

        if (!string.IsNullOrEmpty(entry.Pages))
            journalPart += $", pp. {entry.Pages}";

        var result = $"{authorString} ({year}) {entry.Title}. {journalPart}.";

        if (!string.IsNullOrEmpty(entry.DigitalObjectIdentifier))
            result += $" https://doi.org/{entry.DigitalObjectIdentifier}";

        return result;
    }

    private string FormatWebsite(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = authors.Any() ? FormatAuthors(authors) : "Unknown Author";
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        var result = $"{authorString} ({year}) {entry.Title}.";

        if (!string.IsNullOrEmpty(entry.Url))
            result += $" Available at: {entry.Url}";

        if (entry.AccessDate.HasValue)
            result += $" (Accessed: {entry.AccessDate.Value:d MMMM yyyy})";

        return result;
    }

    private string FormatReport(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        var result = $"{authorString} ({year}) {entry.Title}. Technical Report.";

        if (!string.IsNullOrEmpty(entry.Publisher))
            result += $" {entry.Publisher}.";

        if (!string.IsNullOrEmpty(entry.DigitalObjectIdentifier))
            result += $" https://doi.org/{entry.DigitalObjectIdentifier}";
        else if (!string.IsNullOrEmpty(entry.Url))
            result += $" Available at: {entry.Url}";

        return result;
    }

    private string FormatDefault(BibliographyEntry entry)
    {
        var authors = entry.Contributors
            .Where(c => c.Role == ContributorRole.Author)
            .ToList();

        var authorString = FormatAuthors(authors);
        var year = entry.PublicationDate?.Year ?? DateTime.Now.Year;

        return $"{authorString} ({year}) {entry.Title}.";
    }
    private string FormatAuthors(List<Contributor> authors)
    {
        if (authors.Count == 0) return "Unknown Author";
        string GetHarvardName(Contributor a) =>  $"{a.LastName}, {(String.IsNullOrEmpty(a.FirstName) ? String.Empty :a.FirstName?[0])}.";

        if (authors.Count >= 4)
        {
            return $"{GetHarvardName(authors[0])} et al.";
        }

        if (authors.Count == 1) return GetHarvardName(authors[0]);

        var names = authors.Select(GetHarvardName).ToList();
        if (authors.Count == 2) return $"{names[0]} and {names[1]}";

        return $"{string.Join(", ", names.SkipLast(1))} and {names.Last()}";
    }
}