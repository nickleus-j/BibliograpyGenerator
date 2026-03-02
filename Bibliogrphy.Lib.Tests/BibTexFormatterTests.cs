using System;
using System.Collections.Generic;
using Xunit;
using Bibliography.Lib.Formatters;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Tests
{
    public class BibTexFormatterTests
    {
        private readonly BibTexFormatter _formatter = BibTexFormatter.GetInstance();

        [Fact]
        public void GetInstance_ReturnsSameInstance()
        {
            var a = BibTexFormatter.GetInstance();
            var b = BibTexFormatter.GetInstance();
            Assert.Same(a, b);
        }

        [Fact]
        public void ToBibTeX_NullEntries_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.ToBibTeX(null!));
        }

        [Fact]
        public void ToBibTeX_WithNullEntryInCollection_ThrowsArgumentNullException()
        {
            var list = new List<BibliographyEntry?> { null };
            Assert.Throws<ArgumentNullException>(() => _formatter.ToBibTeX(list! as IEnumerable<BibliographyEntry>));
        }

        [Fact]
        public void ToBibTeX_Book_IncludesAuthorTitleYearPublisherAndDoi_NoUrl()
        {
            var entry = new BibliographyEntry
            {
                SourceType = SourceType.Book,
                Title = "A Special-Book:Title.",
                Publisher = "Test Publisher",
                DigitalObjectIdentifier = "10.1234/abcd",
                PublicationDate = new PublicationDate { Year = 2021, Month = 5, Day = 10 }
            };
            entry.Contributors.Add(new Contributor { FirstName = "John", LastName = "Doe", Role = ContributorRole.Author });

            var result = _formatter.ToBibTeX(new[] { entry });

            Assert.Contains("@book{", result);
            Assert.Contains("author = {Doe, John}", result);
            Assert.Contains("title = {A Special-Book:Title.}", result);
            Assert.Contains("year = {2021}", result);
            Assert.Contains("publisher = {Test Publisher}", result);
            Assert.Contains("doi = {10.1234/abcd}", result);
            Assert.DoesNotContain("url =", result);
        }

        [Fact]
        public void ToBibTeX_Journal_IncludesJournalVolumeNumberPagesAndUrl()
        {
            var entry = new BibliographyEntry
            {
                SourceType = SourceType.Journal,
                Title = "Journal Article",
                ContainerTitle = "Journal of Tests",
                Volume = "12",
                Issue = "3",
                Pages = "45-56",
                Url = "http://journal.example",
                PublicationDate = new PublicationDate { Year = 2020 }
            };
            entry.Contributors.Add(new Contributor { FirstName = "Alice", LastName = "Smith", Role = ContributorRole.Author });

            var result = _formatter.ToBibTeX(new[] { entry });

            Assert.Contains("@article{", result);
            Assert.Contains("journal = {Journal of Tests}", result);
            Assert.Contains("volume = {12}", result);
            Assert.Contains("number = {3}", result);
            Assert.Contains("pages = {45-56}", result);
            // For non-website entries, URL should be emitted if DOI not present
            Assert.Contains("url = {http://journal.example}", result);
        }

        [Fact]
        public void ToBibTeX_Website_IncludesHowPublishedAndAccessNote_NoSeparateUrlField()
        {
            var entry = new BibliographyEntry
            {
                SourceType = SourceType.Website,
                Title = "Website Title",
                Url = "http://example.com",
                AccessDate = new DateOnly(2024, 1, 2),
                PublicationDate = new PublicationDate { Year = 2024 }
            };
            entry.Contributors.Add(new Contributor { FirstName = "Jane", LastName = "Public", Role = ContributorRole.Author });

            var result = _formatter.ToBibTeX(new[] { entry });

            Assert.Contains("@misc{", result); // website maps to misc
            Assert.Contains(@"howpublished = {\url{http://example.com}}", result);
            Assert.Contains("note = {Accessed: 2024-01-02}", result);
            // Website should not include an additional top-level url field
            Assert.DoesNotContain("url = {http://example.com}", result);
        }

        [Fact]
        public void ToBibTeX_Report_IncludesInstitution_WhenPublisherPresent()
        {
            var entry = new BibliographyEntry
            {
                SourceType = SourceType.Report,
                Title = "Tech Report",
                Publisher = "Research Lab",
                PublicationDate = new PublicationDate { Year = 2019 }
            };
            entry.Contributors.Add(new Contributor { FirstName = "Eve", LastName = "Adams", Role = ContributorRole.Author });

            var result = _formatter.ToBibTeX(new[] { entry });

            Assert.Contains("@techreport{", result);
            Assert.Contains("institution = {Research Lab}", result);
        }
    }
}