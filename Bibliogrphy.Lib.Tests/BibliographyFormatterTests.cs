using Bibliography.Lib.Models;
using Bibliography.Lib.Formatters;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bibliography.Lib.Tests.Formatters
{
    public class BibliographyFormatterTests
    {
        #region Setup & Helpers

        private BibliographyFormatter GetFormatter()
        {
            return BibliographyFormatter.GetInstance();
        }

        private BibliographyEntry CreateTestEntry(
            string title = "Test Title",
            string publisher = "Test Publisher",
            int year = 2023,
            CitationStyle style = CitationStyle.APA,
            List<Contributor> contributors = null)
        {
            return new BibliographyEntry
            {
                Title = title,
                Publisher = publisher,
                PublicationDate = new PublicationDate{Day = 3, Month = 4, Year = year},
                CitationStyle = style,
                Contributors = contributors ?? new List<Contributor>
                {
                    new Contributor { FirstName = "John", LastName = "Doe", Role = ContributorRole.Author }
                }
            };
        }

        private Contributor CreateContributor(string firstName, string lastName, ContributorRole role)
        {
            return new Contributor { FirstName = firstName, LastName = lastName, Role = role };
        }

        #endregion

        #region GetInstance Tests

        [Fact]
        public void GetInstance_ReturnsSameInstance_WhenCalledMultipleTimes()
        {
            // Arrange & Act
            var instance1 = BibliographyFormatter.GetInstance();
            var instance2 = BibliographyFormatter.GetInstance();

            // Assert
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void GetInstance_ReturnsNotNull()
        {
            // Act
            var formatter = BibliographyFormatter.GetInstance();

            // Assert
            Assert.NotNull(formatter);
        }

        #endregion

        #region FormatBibliography - Basic Tests

        [Fact]
        public void FormatBibliography_ThrowsArgumentNullException_WhenEntriesAreNull()
        {
            // Arrange
            var formatter = GetFormatter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => formatter.FormatBibliography(null));
        }

        [Fact]
        public void FormatBibliography_ReturnsNoEntriesMessage_WhenListIsEmpty()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>();

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Equal("No entries provided.", result);
        }

        [Fact]
        public void FormatBibliography_FormatsMultipleEntries_Correctly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(title: "Book A", year: 2023),
                CreateTestEntry(title: "Book B", year: 2022)
            };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Book A", result);
            Assert.Contains("Book B", result);
        }

        [Fact]
        public void FormatBibliography_ContainsLineBreaks_BetweenEntries()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(title: "First Book"),
                CreateTestEntry(title: "Second Book")
            };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(lines.Length > 2, "Bibliography should have line breaks between entries");
        }

        [Fact]
        public void FormatBibliography_ReturnsTrimmmedResult_NoTrailingWhitespace()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(title: "Only Book")
            };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.False(result.EndsWith(Environment.NewLine), "Result should not end with newline");
        }

        #endregion

        #region FormatBibliography - Sorting Tests

        [Fact]
        public void FormatBibliography_SortsByAuthorLastName_Alphabetically()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(
                    title: "Book Z",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Zoe", "Smith", ContributorRole.Author)
                    }),
                CreateTestEntry(
                    title: "Book A",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Alice", "Anderson", ContributorRole.Author)
                    }),
                CreateTestEntry(
                    title: "Book M",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Michael", "Miller", ContributorRole.Author)
                    })
            };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            var andersonIndex = result.IndexOf("Anderson");
            var millerIndex = result.IndexOf("Miller");
            var smithIndex = result.IndexOf("Smith");

            Assert.True(andersonIndex < millerIndex && millerIndex < smithIndex,
                "Entries should be sorted alphabetically by author last name");
        }

        [Fact]
        public void FormatBibliography_UsesTitle_WhenNoAuthorExists()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(
                    title: "Zebra Encyclopedia",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("John", "Editor", ContributorRole.Editor)
                    }),
                CreateTestEntry(
                    title: "Apple Handbook",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Jane", "Editor", ContributorRole.Editor)
                    })
            };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            var appleIndex = result.IndexOf("Apple");
            var zebraIndex = result.IndexOf("Zebra");
            Assert.True(appleIndex < zebraIndex, "Should sort by title when no author exists");
        }

        [Fact]
        public void FormatBibliography_SortsMixedAuthorsAndTitles_Correctly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(
                    title: "Zebra Book",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("John", "Editor", ContributorRole.Editor),
                        CreateContributor("Bob", "Author", ContributorRole.Author)
                    }),
                CreateTestEntry(
                    title: "Apple Book",
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Bob", "Writer", ContributorRole.Author)
                    })
            };

            // Act
            var result = formatter.FormatBibliography(entries,CitationStyle.MLA);

            // Assert
            var authorIndex = result.IndexOf("Author");
            var editorIndex = result.IndexOf("Editor");
            Assert.True(authorIndex < editorIndex || editorIndex<0, $"Author should come before editor without author");
        }

        #endregion

        #region FormatBibliography - Style Override Tests

        [Fact]
        public void FormatBibliography_AppliesStyleOverride_ToAllEntries()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>
            {
                CreateTestEntry(title: "Book 1", style: CitationStyle.APA),
                CreateTestEntry(title: "Book 2", style: CitationStyle.MLA)
            };

            // Act
            var result = formatter.FormatBibliography(entries, CitationStyle.Harvard);

            // Assert
            Assert.Contains("(2023)", result);
            // Harvard format includes parentheses around year
        }

        [Fact]
        public void FormatBibliography_UsesIndividualStyle_WhenNoOverride()
        {
            // Arrange
            var formatter = GetFormatter();
            var apaEntry = CreateTestEntry(title: "APA Book", style: CitationStyle.APA);
            var mlaEntry = CreateTestEntry(title: "MLA Book", style: CitationStyle.MLA);
            var entries = new List<BibliographyEntry> { apaEntry, mlaEntry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("APA Book", result);
            Assert.Contains("MLA Book", result);
        }

        [Fact]
        public void FormatBibliography_OverrideStyle_NullOverride_UsesEntryStyle()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(title: "Test Book", style: CitationStyle.Chicago);
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries, CitationStyle.Chicago);

            // Assert
            Assert.Contains("Test Book", result);
            Assert.Contains("Test Publisher", result); // Publisher should be included
        }

        #endregion

        #region FormatBibliography - APA Format Tests

        [Fact]
        public void FormatBibliography_APA_SingleAuthor()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Learning C#",
                publisher: "Tech Books",
                year: 2023,
                style: CitationStyle.APA,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Doe, J.", result);
            Assert.Contains("(2023)", result);
            Assert.Contains("Learning C#", result);
            Assert.Contains("Tech Books", result);
        }

        [Fact]
        public void FormatBibliography_APA_TwoAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                style: CitationStyle.APA,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author),
                    CreateContributor("Jane", "Smith", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Doe, J., & Smith, J.", result);
        }

        [Fact]
        public void FormatBibliography_APA_ThreeOrMoreAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                style: CitationStyle.APA,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author),
                    CreateContributor("Jane", "Smith", ContributorRole.Author),
                    CreateContributor("Bob", "Johnson", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Doe, J., et al.", result);
        }

        #endregion

        #region FormatBibliography - MLA Format Tests

        [Fact]
        public void FormatBibliography_MLA_SingleAuthor()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Web Development Basics",
                publisher: "Web Press",
                year: 2023,
                style: CitationStyle.MLA,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Doe, John", result);
            Assert.Contains("Web Development Basics", result);
            Assert.Contains("Web Press", result);
            Assert.Contains("2023", result);
        }

        [Fact]
        public void FormatBibliography_MLA_TwoAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                style: CitationStyle.MLA,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author),
                    CreateContributor("Jane", "Smith", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Doe, John, and Jane Smith", result);
        }

        [Fact]
        public void FormatBibliography_MLA_ThreeOrMoreAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                style: CitationStyle.MLA,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author),
                    CreateContributor("Jane", "Smith", ContributorRole.Author),
                    CreateContributor("Bob", "Johnson", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("Doe, John, et al.", result);
        }

        #endregion

        #region FormatBibliography - IEEE Format Tests

        [Fact]
        public void FormatBibliography_IEEE_SingleAuthor()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Machine Learning Basics",
                publisher: "IEEE Press",
                year: 2023,
                style: CitationStyle.IEEE,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("[1]", result);
            Assert.Contains("J. Doe", result);
            Assert.Contains("Machine Learning Basics", result);
        }

        [Fact]
        public void FormatBibliography_IEEE_TwoAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                style: CitationStyle.IEEE,
                contributors: new List<Contributor>
                {
                    CreateContributor("John", "Doe", ContributorRole.Author),
                    CreateContributor("Jane", "Smith", ContributorRole.Author)
                });
            var entries = new List<BibliographyEntry> { entry };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("J. Doe and J. Smith", result);
        }
        #endregion
    }
}
