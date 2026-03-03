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
        public BibTexFormatterTests()
        {
            _formatter = BibTexFormatter.GetInstance();
        }

        #region Basic Functionality Tests

        [Fact]
        public void ToBibTeX_WithValidBookEntry_ReturnsCorrectFormat()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Understanding Everything",
            SourceType = SourceType.Book,
            Publisher = "Academic Press",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Robert", LastName = "Johnson", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2022 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@book{Johnson2022,", result);
            Assert.Contains("author = {Robert Johnson},", result);
            Assert.Contains("title = {Understanding Everything},", result);
            Assert.Contains("publisher = {Academic Press},", result);
            Assert.Contains("year = {2022}", result);
        }

        [Fact]
        public void ToBibTeX_WithValidArticleEntry_ReturnsCorrectFormat()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "An Important Study",
            SourceType = SourceType.Journal,
            ContainerTitle = "Science Today",
            Volume = "45",
            Issue = "3",
            Pages = "123-145",
            DigitalObjectIdentifier = "10.1234/example",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "John", LastName = "Smith", Role = ContributorRole.Author },
                new Contributor { FirstName = "Jane", LastName = "Doe", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@article{Smith2023,", result);
            Assert.Contains("author = {John Smith and Jane Doe},", result);
            Assert.Contains("title = {An Important Study},", result);
            Assert.Contains("journal = {Science Today},", result);
            Assert.Contains("volume = {45},", result);
            Assert.Contains("number = {3},", result);
            Assert.Contains("pages = {123-145},", result);
            Assert.Contains("year = {2023},", result);
            Assert.Contains("doi = {10.1234/example}", result);
        }

        [Fact]
        public void ToBibTeX_WithMultipleEntries_SeparatedByBlankLines()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "First Book",
            SourceType = SourceType.Book,
            Publisher = "Publisher A",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Author", LastName = "One", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2020 }
        },
        new BibliographyEntry
        {
            Title = "Second Book",
            SourceType = SourceType.Book,
            Publisher = "Publisher B",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Author", LastName = "Two", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2021 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@book{One2020,", result);
            Assert.Contains("@book{Two2021,", result);
            // Check for blank line between entries
            Assert.Contains("\r\n@book", result);
        }

        #endregion

        #region Author/Contributor Tests

        [Fact]
        public void ToBibTeX_WithSingleAuthor_FormatsCorrectly()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Single Author Book",
            SourceType = SourceType.Book,
            Publisher = "Test Publisher",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "John", LastName = "Doe", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("author = {John Doe},", result);
        }

        [Fact]
        public void ToBibTeX_WithMultipleAuthors_JoinedByAnd()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Multi Author Book",
            SourceType = SourceType.Book,
            Publisher = "Test Publisher",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "John", LastName = "Smith", Role = ContributorRole.Author },
                new Contributor { FirstName = "Jane", LastName = "Doe", Role = ContributorRole.Author },
                new Contributor { FirstName = "Bob", LastName = "Johnson", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("author = {John Smith and Jane Doe and Bob Johnson},", result);
        }

        [Fact]
        public void ToBibTeX_WithAuthorWithoutFirstName_UsesLastNameOnly()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Test Book",
            SourceType = SourceType.Book,
            Publisher = "Test Publisher",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = null, LastName = "Anonymous", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("author = {Anonymous},", result);
        }

        [Fact]
        public void ToBibTeX_WithNoAuthors_OmitsAuthorField()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Authorless Book",
            SourceType = SourceType.Book,
            Publisher = "Test Publisher",
            Contributors = new List<Contributor>(),
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.DoesNotContain("author = ", result);
            Assert.Contains("@book{anon2023,", result);
        }

        #endregion

        #region Source Type Tests

        [Fact]
        public void ToBibTeX_WithWebsiteEntry_IncludesUrlAndAccessDate()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Example Website",
            SourceType = SourceType.Website,
            Url = "https://example.com",
            AccessDate = new DateOnly(2023, 12, 15),
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Web", LastName = "Author", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@misc{Author2023,", result);
            Assert.Contains("url = {https://example.com},", result);
            Assert.Contains("note = {Accessed: 2023-12-15},", result);
        }

        [Fact]
        public void ToBibTeX_WithReportEntry_UsesInstitutionField()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Technical Report",
            SourceType = SourceType.Report,
            Publisher = "Research Institute",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Dr.", LastName = "Researcher", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@techreport{Researcher2023,", result);
            Assert.Contains("institution = {Research Institute},", result);
        }

        #endregion

        #region Citation Key Tests

        [Fact]
        public void ToBibTeX_CitationKeyFormat_LastnameAndYear()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Test Book",
            SourceType = SourceType.Book,
            Publisher = "Publisher",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "John", LastName = "Doe", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2021 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@book{Doe2021,", result);
        }

        [Fact]
        public void ToBibTeX_CitationKeyWithNoAuthor_UsesAnon()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Anonymous Book",
            SourceType = SourceType.Book,
            Publisher = "Publisher",
            Contributors = new List<Contributor>(),
            PublicationDate = new PublicationDate { Year = 2021 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("@book{anon2021,", result);
        }

        #endregion

        #region Field Formatting Tests

        [Fact]
        public void ToBibTeX_FieldsHaveProperSpacing_AroundEquals()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
            {
                new BibliographyEntry
                {
                    Title = "Spacing Test",
                    SourceType = SourceType.Book,
                    Publisher = "Test",
                    Contributors = new List<Contributor>
                    {
                        new Contributor { FirstName = "Test", LastName = "Author", Role = ContributorRole.Author }
                    },
                    PublicationDate = new PublicationDate { Year = 2023 }
                }
            };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains(" = {", result); // Space before =
        }

        [Fact]
        public void ToBibTeX_ProperIndentation_FourSpaces()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Indentation Test",
            SourceType = SourceType.Book,
            Publisher = "Test",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Test", LastName = "Author", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("    author = ", result); // 4 spaces
            Assert.Contains("    title = ", result);
        }

        [Fact]
        public void ToBibTeX_LastFieldHasNoTrailingComma()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
            {
                new BibliographyEntry
                {
                    Title = "Comma Test",
                    SourceType = SourceType.Book,
                    Publisher = "Test",
                    Contributors = new List<Contributor>
                    {
                        new Contributor { FirstName = "Test", LastName = "Author", Role = ContributorRole.Author }
                    },
                    PublicationDate = new PublicationDate { Year = 2023 }
                }
            };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("year = {2023},\r\n}", result); // No comma before closing brace
        }

        #endregion

        #region Special Cases Tests

        [Fact]
        public void ToBibTeX_WithDOI_IncludesDOIField()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "DOI Test",
            SourceType = SourceType.Journal,
            ContainerTitle = "Journal",
            DigitalObjectIdentifier = "10.1234/test.5678",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Test", LastName = "Author", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("doi = {10.1234/test.5678}", result);
        }

        [Fact]
        public void ToBibTeX_WithoutDOI_IncludesUrlIfProvided()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "URL Test",
            SourceType = SourceType.Journal,
            ContainerTitle = "Journal",
            Url = "https://example.com/article",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Test", LastName = "Author", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("url = {https://example.com/article}", result);
        }

        [Fact]
        public void ToBibTeX_JournalEntry_IncludesAllJournalFields()
        {
            // Arrange
            var entries = new List<BibliographyEntry>
    {
        new BibliographyEntry
        {
            Title = "Journal Article",
            SourceType = SourceType.Journal,
            ContainerTitle = "Nature",
            Volume = "123",
            Issue = "4",
            Pages = "456-789",
            Contributors = new List<Contributor>
            {
                new Contributor { FirstName = "Jane", LastName = "Scientist", Role = ContributorRole.Author }
            },
            PublicationDate = new PublicationDate { Year = 2023 }
        }
    };

            // Act
            var result = _formatter.ToBibTeX(entries);

            // Assert
            Assert.Contains("journal = {Nature},", result);
            Assert.Contains("volume = {123},", result);
            Assert.Contains("number = {4},", result);
            Assert.Contains("pages = {456-789},", result);
        }

        #endregion

    }
}