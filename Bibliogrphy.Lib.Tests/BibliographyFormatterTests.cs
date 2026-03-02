using System;
using System.Linq;
using Xunit;
using Bibliography.Lib.Formatters;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Tests
{
    public class BibliographyFormatterTests
    {
        private readonly BibliographyFormatter _formatter = BibliographyFormatter.GetInstance();

        [Fact]
        public void FormatCitation_NullEntry_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatCitation(null!));
        }

        [Fact]
        public void FormatBibliography_NullEntries_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatBibliography(null!, CitationStyle.APA));
        }

        [Fact]
        public void FormatCitation_APA_IncludesContributorsDateTitlePublisherDOIAndUrl()
        {
            var entry = new BibliographyEntry
            {
                Title = "Test Title",
                Publisher = "Test Publisher",
                DigitalObjectIdentifier = "10.1234/abcd",
                Url = "http://example.com",
                PublicationDate = new PublicationDate { Year = 2021, Month = 5, Day = 10 },
                CitationStyle = CitationStyle.APA
            };
            entry.Contributors.Add(new Contributor { FirstName = "John", LastName = "Doe", Role = ContributorRole.Author });

            var result = _formatter.FormatCitation(entry);

            Assert.Contains("Doe, John (Author)", result);
            Assert.Contains("(2021-05-10)", result);
            Assert.Contains("Test Title", result);
            Assert.Contains("Test Publisher", result);
            Assert.Contains("doi:10.1234/abcd", result);
            Assert.Contains("Retrieved from http://example.com", result);
        }

        [Fact]
        public void FormatBibliography_SortsByContributorLastNameThenYear()
        {
            var e1 = new BibliographyEntry
            {
                Title = "First",
                PublicationDate = new PublicationDate { Year = 2020 },
                CitationStyle = CitationStyle.APA
            };
            e1.Contributors.Add(new Contributor { FirstName = "Alice", LastName = "Smith", Role = ContributorRole.Author });

            var e2 = new BibliographyEntry
            {
                Title = "Second",
                PublicationDate = new PublicationDate { Year = 2021 },
                CitationStyle = CitationStyle.APA
            };
            e2.Contributors.Add(new Contributor { FirstName = "Bob", LastName = "Adams", Role = ContributorRole.Author });

            // Pass in reverse order to ensure sorting actually occurs
            var bibliography = _formatter.FormatBibliography(new[] { e1, e2 }, CitationStyle.APA);

            var indexAdams = bibliography.IndexOf("Adams,");
            var indexSmith = bibliography.IndexOf("Smith,");

            Assert.True(indexAdams >= 0, "Adams entry should be present");
            Assert.True(indexSmith >= 0, "Smith entry should be present");
            Assert.True(indexAdams < indexSmith, "Entries should be ordered by contributor last name ascending");
        }

        [Fact]
        public void FormatCitation_Harvard_IncludesAccessDateWhenPresent()
        {
            var entry = new BibliographyEntry
            {
                Title = "Webpage Title",
                Url = "http://site.example",
                AccessDate = new DateOnly(2023, 3, 15),
                CitationStyle = CitationStyle.Harvard
            };
            entry.Contributors.Add(new Contributor { FirstName = "Jane", LastName = "Public", Role = ContributorRole.Author });

            var result = _formatter.FormatCitation(entry);

            Assert.Contains("Public, Jane (Author)", result);
            Assert.Contains("Available at: http://site.example", result);
            Assert.Contains($"(Accessed: {entry.AccessDate.Value:yyyy-MM-dd})", result);
        }
    }
}