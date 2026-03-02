using Bibliography.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bibliography.Lib.Formatters
{
    public class BibliographyFormatter
    {
        #region Properties & Constructor

        private static BibliographyFormatter Instance { get; set; }

        private BibliographyFormatter() { }

        #endregion

        #region Public Methods

        public static BibliographyFormatter GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BibliographyFormatter();
            }
            return Instance;
        }

        public string FormatBibliography(IEnumerable<BibliographyEntry> entries, CitationStyle? styleOverride = null)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (!entries.Any()) return "No entries provided.";

            var sortedEntries = styleOverride== CitationStyle.IEEE? entries.ToList(): entries
                .OrderBy(e => e.Contributors.FirstOrDefault(c => c.Role == ContributorRole.Author)?.LastName ?? e.Title)
                .ToList();

            var sb = new StringBuilder();

            for(int i=0;i< sortedEntries.Count;i++)
            {
                var entry = sortedEntries.ElementAt(i);
                var targetStyle = styleOverride ?? entry.CitationStyle;

                string formattedEntry = targetStyle switch
                {
                    CitationStyle.APA => FormatApa(entry),
                    CitationStyle.MLA => FormatMla(entry),
                    CitationStyle.IEEE => FormatIeee(entry,i+1),
                    CitationStyle.Chicago => FormatChicago(entry),
                    CitationStyle.Harvard => FormatHarvard(entry),
                    _ => FormatApa(entry)
                };

                sb.AppendLine(formattedEntry);
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Private Methods - Style Formatters

        private string FormatApa(BibliographyEntry entry)
        {
            var authorString = FormatAuthors(entry, CitationStyle.APA);
            var year = entry.PublicationDate?.Year.ToString() ?? "n.d.";
            return $"{authorString} ({year}). {entry.Title}. {entry.Publisher}.";
        }

        private string FormatMla(BibliographyEntry entry)
        {
            var authorString = FormatAuthors(entry, CitationStyle.MLA);
            return $"{authorString}. {entry.Title}. {entry.Publisher}, {entry.PublicationDate?.Year}.";
        }

        private string FormatIeee(BibliographyEntry entry,int index)
        {
            var authorString = FormatAuthors(entry, CitationStyle.IEEE);
            return $"[{index}] {authorString}, \"{entry.Title},\" {entry.Publisher}, {entry.PublicationDate?.Year}.";
        }

        private string FormatChicago(BibliographyEntry entry)
        {
            var authorString = FormatAuthors(entry, CitationStyle.Chicago);
            return $"{authorString} {entry.Title}. {entry.Publisher}, {entry.PublicationDate?.Year}.";
        }

        private string FormatHarvard(BibliographyEntry entry)
        {
            var authorString = FormatAuthors(entry, CitationStyle.Harvard);
            return $"{authorString} ({entry.PublicationDate?.Year}) {entry.Title}. {entry.Publisher}.";
        }

        #endregion

        #region Private Methods - Author Formatting

        private string FormatAuthors(BibliographyEntry entry, CitationStyle style)
        {
            var authors = entry.Contributors
                .Where(c => c.Role == ContributorRole.Author)
                .ToList();

            if (authors.Count == 0) return "Unknown Author";

            return style switch
            {
                CitationStyle.APA => FormatApaAuthors(authors),
                CitationStyle.MLA => FormatMlaAuthors(authors),
                CitationStyle.IEEE => FormatIeeeAuthors(authors),
                CitationStyle.Chicago => FormatChicagoAuthors(authors),
                CitationStyle.Harvard => FormatHarvardAuthors(authors),
                _ => $"{authors[0].LastName} et al."
            };
        }

        private string FormatApaAuthors(List<Contributor> authors)
        {
            if (authors.Count == 1)
                return $"{authors[0].LastName}, {authors[0].FirstName?[0]}.";

            if (authors.Count == 2)
                return $"{authors[0].LastName}, {authors[0].FirstName?[0]}., & {authors[1].LastName}, {authors[1].FirstName?[0]}.";

            return $"{authors[0].LastName}, {authors[0].FirstName?[0]}., et al.";
        }

        private string FormatMlaAuthors(List<Contributor> authors)
        {
            if (authors.Count == 1)
                return $"{authors[0].LastName}, {authors[0].FirstName}.";

            if (authors.Count == 2)
                return $"{authors[0].LastName}, {authors[0].FirstName}, and {authors[1].FirstName} {authors[1].LastName}.";

            return $"{authors[0].LastName}, {authors[0].FirstName}, et al.";
        }

        private string FormatIeeeAuthors(List<Contributor> authors)
        {
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

        private string FormatChicagoAuthors(List<Contributor> authors)
        {
            if (authors.Count >= 10)
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

        private string FormatHarvardAuthors(List<Contributor> authors)
        {
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

        #endregion

        #region Private Methods - Helpers
        private string GetIeeeName(Contributor author)
        {
            if (string.IsNullOrWhiteSpace(author.FirstName))
                return author.LastName;

            return $"{author.FirstName[0]}. {author.LastName}";
        }

        #endregion
    }
}
