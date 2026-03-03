using Bibliography.Lib.Formatters;
using Bibliography.Lib.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bibliography.Lib.Tests.Formatters
{
    public class BibliographyFormatterTests
    {
        #region Setup & Helpers
        private readonly BibliographyFormatter _formatter;

        public BibliographyFormatterTests()
        {
            _formatter = GetFormatter();
        }
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
                PublicationDate = new PublicationDate { Day = 3, Month = 4, Year = year },
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
            Assert.Throws<ArgumentNullException>(() => formatter.FormatBibliography(null,CitationStyle.APA));
        }

        [Fact]
        public void FormatBibliography_ReturnsNoEntriesMessage_WhenListIsEmpty()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new List<BibliographyEntry>();

            // Act
            var result = formatter.FormatBibliography(entries,CitationStyle.APA);

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
            var result = formatter.FormatBibliography(entries,CitationStyle.APA);

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
            var result = formatter.FormatBibliography(entries,CitationStyle.APA);

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
            var result = formatter.FormatBibliography(entries,CitationStyle.APA);

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
            var result = formatter.FormatBibliography(entries,CitationStyle.APA);

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
            var result = formatter.FormatBibliography(entries, CitationStyle.MLA);

            // Assert
            var authorIndex = result.IndexOf("Author");
            var editorIndex = result.IndexOf("Editor");
            Assert.True(authorIndex < editorIndex || editorIndex < 0, $"Author should come before editor without author");
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

        #region IEEE Single Author Tests

        [Fact]
        public void FormatBibliography_IeeeWithSingleAuthor_ReturnsCorrectFormat()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Introduction to Networks",
                publisher: "Cisco Press",
                year: 2020,
                style: CitationStyle.IEEE,
                contributors: new List<Contributor>
                {
                CreateContributor("Andrew", "Tanenbaum", ContributorRole.Author)
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("[1]", result);
            Assert.Contains("A. Tanenbaum", result);
            Assert.Contains("\"Introduction to Networks,\"", result);
            Assert.Contains("Cisco Press", result);
            Assert.Contains("2020", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithSingleAuthorNoFirstName_ReturnsLastNameOnly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                new Contributor { FirstName = "", LastName = "Anonymous", Role = ContributorRole.Author }
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.IEEE);

            // Assert
            Assert.Contains("[1] Anonymous,", result);
        }

        #endregion

        #region IEEE Two Authors Tests

        [Fact]
        public void FormatBibliography_IeeeWithTwoAuthors_ReturnsCorrectFormat()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Computer Networking",
                publisher: "Pearson",
                year: 2019,
                contributors: new List<Contributor>
                {
                CreateContributor("James", "Kurose", ContributorRole.Author),
                CreateContributor("Keith", "Ross", ContributorRole.Author)
                }, style: CitationStyle.IEEE
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("[1]", result);
            Assert.Contains("J. Kurose", result);
            Assert.Contains("and K. Ross", result);
            Assert.DoesNotContain("et al.", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithTwoAuthorsAndNonAuthorContributor_IgnoresNonAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("John", "Smith", ContributorRole.Author),
                CreateContributor("Jane", "Doe", ContributorRole.Author),
                CreateContributor("Editor", "Name", ContributorRole.Editor)
                }, style: CitationStyle.IEEE
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("J. Smith and J. Doe", result);
            Assert.DoesNotContain("Editor", result);
        }

        #endregion

        #region IEEE Three to Six Authors Tests

        [Fact]
        public void FormatBibliography_IeeeWithThreeAuthors_ReturnsCorrectFormat()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("Alice", "Johnson", ContributorRole.Author),
                CreateContributor("Bob", "Smith", ContributorRole.Author),
                CreateContributor("Charlie", "Brown", ContributorRole.Author)
                }, style: CitationStyle.IEEE
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("[1]", result);
            Assert.Contains("A. Johnson, B. Smith, and C. Brown", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithFourAuthors_ReturnsCorrectFormat()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("Author", "One", ContributorRole.Author),
                CreateContributor("Author", "Two", ContributorRole.Author),
                CreateContributor("Author", "Three", ContributorRole.Author),
                CreateContributor("Author", "Four", ContributorRole.Author)
                }, style: CitationStyle.IEEE
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert 
            Assert.Contains("A. One, A. Two, A. Three, and A. Four", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithSixAuthors_ReturnsAllAuthorsWithoutEtAl()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("First", "One", ContributorRole.Author),
                CreateContributor("Second", "Two", ContributorRole.Author),
                CreateContributor("Third", "Three", ContributorRole.Author),
                CreateContributor("Fourth", "Four", ContributorRole.Author),
                CreateContributor("Fifth", "Five", ContributorRole.Author),
                CreateContributor("Sixth", "Six", ContributorRole.Author)
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.IEEE);

            // Assert
            Assert.Contains("F. One, S. Two, T. Three, F. Four, F. Five, and S. Six", result);
            Assert.DoesNotContain("et al.", result);
        }

        #endregion

        #region IEEE Seven or More Authors Tests

        [Fact]
        public void FormatBibliography_IeeeWithSevenAuthors_ReturnsEtAl()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Large Collaborative Research",
                contributors: new List<Contributor>
                {
                CreateContributor("First", "Author", ContributorRole.Author),
                CreateContributor("Second", "Author", ContributorRole.Author),
                CreateContributor("Third", "Author", ContributorRole.Author),
                CreateContributor("Fourth", "Author", ContributorRole.Author),
                CreateContributor("Fifth", "Author", ContributorRole.Author),
                CreateContributor("Sixth", "Author", ContributorRole.Author),
                CreateContributor("Seventh", "Author", ContributorRole.Author)
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.IEEE);

            // Assert
            Assert.Contains("[1]", result);
            Assert.Contains("F. Author et al.", result);
            Assert.DoesNotContain("Second", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithTenAuthors_ReturnsEtAlWithFirstAuthorOnly()
        {
            // Arrange
            var formatter = GetFormatter();
            var contributors = Enumerable.Range(1, 10)
                .Select(i => CreateContributor($"Author{i}", $"Last{i}", ContributorRole.Author))
                .ToList();

            var entry = CreateTestEntry(contributors: contributors);

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.IEEE);

            // Assert
            Assert.Contains("A. Last1 et al.", result);
            Assert.DoesNotContain("Author2", result);
        }

        #endregion

        #region IEEE Citation Number Tests

        [Fact]
        public void FormatBibliography_IeeeWithMultipleEntries_AssignsCorrectCitationNumbers()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(title: "First Book", contributors: new List<Contributor>
            {
                CreateContributor("John", "Smith", ContributorRole.Author)
            }),
            CreateTestEntry(title: "Second Book", contributors: new List<Contributor>
            {
                CreateContributor("Jane", "Doe", ContributorRole.Author)
            }),
            CreateTestEntry(title: "Third Book", contributors: new List<Contributor>
            {
                CreateContributor("Bob", "Johnson", ContributorRole.Author)
            })
        };

            // Act
            var result = formatter.FormatBibliography(entries, CitationStyle.IEEE);

            // Assert
            Assert.Contains("[1] J. Smith", result);
            Assert.Contains("[2] J. Doe", result);
            Assert.Contains("[3] B. Johnson", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithSingleEntry_StartsWithBracketOne()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry();

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.IEEE);

            // Assert
            Assert.StartsWith("[1]", result);
        }

        #endregion

        #region IEEE Title and Publisher Tests

        [Fact]
        public void FormatBibliography_IeeeWithTitleContainingSpecialCharacters_IncludesInQuotes()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(title: "C++ Programming: Principles & Practice");

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.IEEE);

            // Assert
            Assert.Contains("\"C++ Programming: Principles & Practice,\"", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithPublisher_IncludedInOutput()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(publisher: "MIT Press");

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("MIT Press", result);
        }

        [Fact]
        public void FormatBibliography_IeeeWithYear_IncludedAtEnd()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(year: 2021, style: CitationStyle.IEEE);

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.EndsWith("2021.", result);
        }

        #endregion

        #region IEEE Sorting Tests

        [Fact]
        public void FormatBibliography_IeeeWithUnsortedEntries_SortsByAuthorLastName()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(title: "Zebra Book", contributors: new List<Contributor>
            {
                CreateContributor("Zoe", "Zhang", ContributorRole.Author)
            }),
            CreateTestEntry(title: "Apple Book", contributors: new List<Contributor>
            {
                CreateContributor("Alice", "Adams", ContributorRole.Author)
            }),
            CreateTestEntry(title: "Mango Book", contributors: new List<Contributor>
            {
                CreateContributor("Mike", "Miller", ContributorRole.Author)
            })
        };

            // Act
            var result = formatter.FormatBibliography(entries, CitationStyle.IEEE);

            // Assert
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.Contains("A. Adams", lines[2]);
            Assert.Contains("M. Miller", lines[4]);
            Assert.Contains("Z. Zhang", lines[0]);
        }

        [Fact]
        public void FormatBibliography_IeeeWithNoAuthor_SortsByTitle()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(title: "Zebra Title", contributors: new List<Contributor>
            {
                CreateContributor("Name", "Contributor", ContributorRole.Editor)
            }),
            CreateTestEntry(title: "Apple Title", contributors: new List<Contributor>
            {
                CreateContributor("Name", "Contributor", ContributorRole.Editor)
            })
        };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.Contains("Apple Title", lines[0]);
            Assert.Contains("Zebra Title", lines[2]);
        }

        #endregion

        #region IEEE Format Structure Tests


        [Fact]
        public void FormatBibliography_IeeeWithMultipleEntries_SeparatedByBlankLines()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(title: "First"),
            CreateTestEntry(title: "Second")
        };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains(Environment.NewLine + Environment.NewLine, result);
        }

        #endregion

        #region IEEE Edge Cases

        [Fact]
        public void FormatBibliography_IeeeWithNullYear_HandlesGracefully()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = new BibliographyEntry
            {
                Title = "Test Title",
                Publisher = "Test Publisher",
                PublicationDate = null,
                CitationStyle = CitationStyle.IEEE,
                Contributors = new List<Contributor>
            {
                CreateContributor("John", "Doe", ContributorRole.Author)
            }
            };

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("[1]", result);
            // Should handle null year gracefully
            Assert.DoesNotContain("null", result.ToLower());
        }

        [Fact]
        public void FormatBibliography_IeeeWithEmptyAuthorList_ReturnsUnknownAuthor()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(contributors: new List<Contributor>
        {
            CreateContributor("Editor", "Name", ContributorRole.Editor)
        });

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Unknown Author", result);
        }

        #endregion
        #endregion
        #region Harvard Single Author Tests

        [Fact]
        public void FormatBibliography_HarvardWithSingleAuthor_ReturnsCorrectFormat()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Introduction to Networks",
                publisher: "Cisco Press",
                year: 2020,
                style: CitationStyle.Harvard,
                contributors: new List<Contributor>
                {
                CreateContributor("Andrew", "Tanenbaum", ContributorRole.Author)
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Tanenbaum, A.", result);
            Assert.Contains("(2020)", result);
            Assert.Contains("Introduction to Networks", result);
            Assert.Contains("Cisco Press", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithSingleAuthorNoFirstName_ReturnsLastNameOnly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                new Contributor { FirstName = "", LastName = "Anonymous", Role = ContributorRole.Author }
                }, style: CitationStyle.Harvard
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Anonymous,", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithSingleAuthorSingleLetterFirstName_FormatsCorrectly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("J", "Smith", ContributorRole.Author)
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Smith, J.", result);
        }

        #endregion

        #region Harvard Two Authors Tests

        [Fact]
        public void FormatBibliography_HarvardWithTwoAuthors_ReturnsCorrectFormat()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Computer Networking",
                publisher: "Pearson",
                year: 2019,
                contributors: new List<Contributor>
                {
                CreateContributor("James", "Kurose", ContributorRole.Author),
                CreateContributor("Keith", "Ross", ContributorRole.Author)
                }
                , style: CitationStyle.Harvard
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Kurose, J. and Ross, K.", result);
            Assert.Contains("(2019)", result);
            Assert.Contains("Computer Networking", result);
            Assert.DoesNotContain("et al.", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithTwoAuthorsAndNonAuthorContributor_IgnoresNonAuthors()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("John", "Smith", ContributorRole.Author),
                CreateContributor("Jane", "Doe", ContributorRole.Author),
                CreateContributor("Editor", "Name", ContributorRole.Editor)
                }, style: CitationStyle.Harvard
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Smith, J. and Doe, J.", result);
            Assert.DoesNotContain("Editor", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithTwoAuthorsWithDifferentFirstLetters_FormatsCorrectly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("Alexander", "Brown", ContributorRole.Author),
                CreateContributor("Benjamin", "Green", ContributorRole.Author)
                }, style: CitationStyle.Harvard
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Brown, A. and Green, B.", result);
        }

        #endregion

        #region Harvard Three Authors Tests

        [Fact]
        public void FormatBibliography_HarvardWithThreeAuthors_DoesNotReturnsEtAl()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Advanced Research Methods",
                contributors: new List<Contributor>
                {
                CreateContributor("Alice", "Johnson", ContributorRole.Author),
                CreateContributor("Bob", "Smith", ContributorRole.Author),
                CreateContributor("Charlie", "Brown", ContributorRole.Author),
                }, style: CitationStyle.Harvard
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.DoesNotContain("Johnson, A. et al.", result);
            Assert.Contains("Smith", result);
            Assert.Contains("Brown", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithThreeAuthorsOnlyFirstAuthorShown_VerifiesEtAlUsage()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("First", "Author", ContributorRole.Author),
                CreateContributor("Second", "Author", ContributorRole.Author),
                CreateContributor("Third", "Author", ContributorRole.Author)
                }, style: CitationStyle.Harvard
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Author, F.", result);
            Assert.Contains("S.", result);
        }

        #endregion

        #region Harvard Four or More Authors Tests

        [Fact]
        public void FormatBibliography_HarvardWithFourAuthors_ReturnsEtAl()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                contributors: new List<Contributor>
                {
                CreateContributor("Author", "One", ContributorRole.Author),
                CreateContributor("Author", "Two", ContributorRole.Author),
                CreateContributor("Author", "Three", ContributorRole.Author),
                CreateContributor("Author", "Four", ContributorRole.Author)
                }
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.Harvard);

            // Assert
            Assert.Contains("One, A. et al.", result);
            Assert.DoesNotContain("Two", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithTenAuthors_ReturnsEtAlWithFirstAuthorOnly()
        {
            // Arrange
            var formatter = GetFormatter();
            var contributors = Enumerable.Range(1, 10)
                .Select(i => CreateContributor($"Author{i}", $"Last{i}", ContributorRole.Author))
                .ToList();

            var entry = CreateTestEntry(contributors: contributors);

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.Harvard);

            // Assert
            Assert.Contains("Last1, A. et al.", result);
            Assert.DoesNotContain("Author2", result);
        }

        #endregion

        #region Harvard Year Formatting Tests

        [Fact]
        public void FormatBibliography_HarvardWithYear_IncludedInParenthesesAfterAuthor()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(year: 2021);

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("(2021)", result);
            Assert.Contains("Doe, J. (2021).", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithDifferentYears_FormatsEachCorrectly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(year: 2015),
            CreateTestEntry(year: 2020),
            CreateTestEntry(year: 2023)
        };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            Assert.Contains("(2015)", result);
            Assert.Contains("(2020)", result);
            Assert.Contains("(2023)", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithNullYear_HandlesGracefully()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = new BibliographyEntry
            {
                Title = "Test Title",
                Publisher = "Test Publisher",
                PublicationDate = null,
                CitationStyle = CitationStyle.Harvard,
                Contributors = new List<Contributor>
            {
                CreateContributor("John", "Doe", ContributorRole.Author)
            }
            };

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Doe, J.", result);
            // Should not contain null or empty parentheses
            Assert.DoesNotContain("(null)", result);
        }

        #endregion

        #region Harvard Title and Publisher Tests

        [Fact]
        public void FormatBibliography_HarvardWithTitle_IncludedInOutput()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(title: "The Art of Computer Programming");

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("The Art of Computer Programming", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithPublisher_IncludedAtEnd()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(publisher: "Oxford University Press");

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("Oxford University Press", result);
        }

        [Fact]
        public void FormatBibliography_HarvardWithTitleContainingSpecialCharacters_IncludedCorrectly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(title: "C++ Programming: Principles & Practice");

            // Act
            var result = formatter.FormatBibliography(new[] { entry });

            // Assert
            Assert.Contains("C++ Programming: Principles & Practice", result);
        }

        [Fact]
        public void FormatBibliography_HarvardFormatStructure_FollowsProperOrder()
        {
            // Arrange
            var formatter = GetFormatter();
            var entry = CreateTestEntry(
                title: "Sample Book",
                publisher: "Sample Publisher",
                year: 2022
            );

            // Act
            var result = formatter.FormatBibliography(new[] { entry }, CitationStyle.Harvard);

            // Assert
            // Format: Author, A. (Year) Title. Publisher.
            Assert.Equal(@"Doe, J. (2022) Sample Book. Sample Publisher.", result);
        }

        #endregion

        #region Harvard Sorting Tests

        [Fact]
        public void FormatBibliography_HarvardWithUnsortedEntries_SortsByAuthorLastName()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(title: "Zebra Book", contributors: new List<Contributor>
            {
                CreateContributor("Zoe", "Zhang", ContributorRole.Author)
            }),
            CreateTestEntry(title: "Apple Book", contributors: new List<Contributor>
            {
                CreateContributor("Alice", "Adams", ContributorRole.Author)
            }),
            CreateTestEntry(title: "Mango Book", contributors: new List<Contributor>
            {
                CreateContributor("Mike", "Miller", ContributorRole.Author)
            })
        };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.Contains("Adams, A.", lines[0]);
            Assert.Contains("Miller, M.", lines[2]);
            Assert.Contains("Zhang, Z.", lines[4]);
        }

        [Fact]
        public void FormatBibliography_HarvardWithNoAuthor_SortsByTitle()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(title: "Zebra Title", contributors: new List<Contributor>
            {
                CreateContributor("Name", "Contributor", ContributorRole.Editor)
            }),
            CreateTestEntry(title: "Apple Title", contributors: new List<Contributor>
            {
                CreateContributor("Name", "Contributor", ContributorRole.Editor)
            })
        };

            // Act
            var result = formatter.FormatBibliography(entries, CitationStyle.Harvard);

            // Assert
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.Contains("Apple Title", lines[0]);
            Assert.Contains("Zebra Title", lines[2]);
        }

        [Fact]
        public void FormatBibliography_HarvardWithSameAuthorMultipleYears_SortsByAuthorThenYear()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
            CreateTestEntry(year: 2023, contributors: new List<Contributor>
            {
                CreateContributor("John", "Smith", ContributorRole.Author)
            }),
            CreateTestEntry(year: 2020, contributors: new List<Contributor>
            {
                CreateContributor("John", "Smith", ContributorRole.Author)
            }),
            CreateTestEntry(year: 2021, contributors: new List<Contributor>
            {
                CreateContributor("John", "Smith", ContributorRole.Author)
            })
        };

            // Act
            var result = formatter.FormatBibliography(entries);

            // Assert
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            // All should be same author, sorted by year in the order they appear (sorting by author then title)
            Assert.All(lines.Where(l => !string.IsNullOrEmpty(l)), line =>
                Assert.Contains("Smith, J.", line));
        }
        [Fact]
        public void FormatBibliography_HarvardWithMultipleEntriesVariousFormats_EachFormattedCorrectly()
        {
            // Arrange
            var formatter = GetFormatter();
            var entries = new[]
            {
                CreateTestEntry(
                    title: "Single Author Book",
                    publisher: "Publisher A",
                    year: 2018,
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Single", "Author", ContributorRole.Author)
                    }),
                CreateTestEntry(
                    title: "Dual Author Book",
                    publisher: "Publisher B",
                    year: 2019,
                    contributors: new List<Contributor>
                    {
                        CreateContributor("First", "Co-Author", ContributorRole.Author),
                        CreateContributor("Second", "Co-Author", ContributorRole.Author)
                    }),
                CreateTestEntry(
                    title: "Multiple Author Book",
                    publisher: "Publisher C",
                    year: 2020,
                    contributors: new List<Contributor>
                    {
                        CreateContributor("Lead", "Researcher", ContributorRole.Author),
                        CreateContributor("Supporting", "Researcher", ContributorRole.Author),
                        CreateContributor("Assisst", "Writer", ContributorRole.Author),
                        CreateContributor("Junior", "Researcher", ContributorRole.Author)
                    })
            };

            // Act
            var result = formatter.FormatBibliography(entries,CitationStyle.Harvard);

            // Assert
            Assert.Contains("Author, S. (2018) Single Author Book. Publisher A.", result);
            Assert.Contains("Co-Author, F. and Co-Author, S. (2019) Dual Author Book. Publisher B.", result);
            Assert.Contains("Researcher, L. et al. (2020) Multiple Author Book. Publisher C.", result);
        }

        #endregion
         [Fact]
    public void FormatChicago_SingleAuthor_FormatsCorrectly()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "The Great Gatsby",
            publisher: "Scribner",
            year: 1925,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>
            {
                CreateContributor("F. Scott", "Fitzgerald", ContributorRole.Author)
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.Equal("Fitzgerald, F. Scott. The Great Gatsby. Scribner, 1925.", result);
    }

    [Fact]
    public void FormatChicago_TwoAuthors_FormatsCorrectly()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "A Collaborative Work",
            publisher: "Academic Press",
            year: 2020,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>
            {
                CreateContributor("Jane", "Smith", ContributorRole.Author),
                CreateContributor("John", "Doe", ContributorRole.Author)
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.Equal("Smith, Jane, and John Doe", result.Split(new[] { "." }, StringSplitOptions.None)[0]);
    }

    [Fact]
    public void FormatChicago_ThreeAuthors_FormatsCorrectly()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "Three Author Book",
            publisher: "Publishing House",
            year: 2021,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>
            {
                CreateContributor("Alice", "Johnson", ContributorRole.Author),
                CreateContributor("Bob", "Williams", ContributorRole.Author),
                CreateContributor("Charlie", "Brown", ContributorRole.Author)
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.Equal("Johnson, Alice, Bob Williams, and Charlie Brown.", result.Split(new[] {" T" }, StringSplitOptions.None)[0]);
    }

    [Fact]
    public void FormatChicago_NineAuthors_FormatsCorrectly()
    {
        // Arrange
        var contributors = new List<Contributor>
        {
            CreateContributor("Author", "One", ContributorRole.Author),
            CreateContributor("Author", "Two", ContributorRole.Author),
            CreateContributor("Author", "Three", ContributorRole.Author),
            CreateContributor("Author", "Four", ContributorRole.Author),
            CreateContributor("Author", "Five", ContributorRole.Author),
            CreateContributor("Author", "Six", ContributorRole.Author),
            CreateContributor("Author", "Seven", ContributorRole.Author),
            CreateContributor("Author", "Eight", ContributorRole.Author),
            CreateContributor("Author", "Nine", ContributorRole.Author)
        };

        var entry = CreateTestEntry(
            title: "Nine Author Work",
            publisher: "Big Publisher",
            year: 2022,
            style: CitationStyle.Chicago,
            contributors: contributors
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.Equal("One, Author, Author Two, Author Three, Author Four, Author Five, Author Six, Author Seven, et al", result.Split(".", StringSplitOptions.None)[0]);
    }

    [Fact]
    public void FormatChicago_TenAuthors_FormatsWithEtAl()
    {
        // Arrange
        var contributors = new List<Contributor>();
        for (int i = 1; i <= 10; i++)
        {
            contributors.Add(CreateContributor($"Author{i}", $"Name{i}", ContributorRole.Author));
        }

        var entry = CreateTestEntry(
            title: "Ten Author Work",
            publisher: "Publisher",
            year: 2023,
            style: CitationStyle.Chicago,
            contributors: contributors
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        var firstLine = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
        Assert.Contains("et al.", firstLine);
        Assert.StartsWith("Name1, Author1", firstLine);
    }

    [Fact]
    public void FormatChicago_NoAuthors_UsesUnknownAuthor()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "Anonymous Work",
            publisher: "Mystery Press",
            year: 2023,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>()
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.StartsWith("Unknown Author", result);
    }

    [Fact]
    public void FormatChicago_WithNullPublicationDate_HandlesGracefully()
    {
        // Arrange
        var entry = new BibliographyEntry
        {
            Title = "Undated Work",
            Publisher = "Some Publisher",
            PublicationDate = null,
            CitationStyle = CitationStyle.Chicago,
            Contributors = new List<Contributor>
            {
                CreateContributor("John", "Smith", ContributorRole.Author)
            }
        };

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.Contains("Smith, John", result);
        Assert.Contains("Undated Work", result);
    }

    [Fact]
    public void FormatChicago_MultipleEntries_SortsAlphabetically()
    {
        // Arrange
        var entries = new[]
        {
            CreateTestEntry(
                title: "Zebra Book",
                publisher: "Publisher",
                year: 2023,
                style: CitationStyle.Chicago,
                contributors: new List<Contributor>
                {
                    CreateContributor("Zachary", "Wilson", ContributorRole.Author)
                }
            ),
            CreateTestEntry(
                title: "Apple Book",
                publisher: "Publisher",
                year: 2023,
                style: CitationStyle.Chicago,
                contributors: new List<Contributor>
                {
                    CreateContributor("Alice", "Anderson", ContributorRole.Author)
                }
            )
        };

        // Act
        var result = _formatter.FormatBibliography(entries);

        // Assert
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        Assert.StartsWith("Anderson, Alice", lines[0]);
        Assert.StartsWith("Wilson, Zachary", lines[1]);
    }

    [Fact]
    public void FormatChicago_WithStyleOverride_UsesOverrideStyle()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "Test Book",
            publisher: "Test Publisher",
            year: 2023,
            style: CitationStyle.APA, // Original style is APA
            contributors: new List<Contributor>
            {
                CreateContributor("John", "Doe", ContributorRole.Author)
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry }, CitationStyle.Chicago);

        // Assert
        Assert.Equal("Doe, John. Test Book. Test Publisher, 2023.", result);
    }

    [Fact]
    public void FormatChicago_AuthorWithoutFirstName_HandlesGracefully()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "Mysterious Work",
            publisher: "Publisher",
            year: 2023,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>
            {
                new Contributor { FirstName = null, LastName = "Mononymous", Role = ContributorRole.Author }
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.Contains("Mononymous", result);
    }

    [Fact]
    public void FormatChicago_FourAuthors_FormatsWithCommaAndAnd()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "Four Author Work",
            publisher: "Publisher",
            year: 2023,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>
            {
                CreateContributor("First", "Author", ContributorRole.Author),
                CreateContributor("Second", "Author", ContributorRole.Author),
                CreateContributor("Third", "Author", ContributorRole.Author),
                CreateContributor("Fourth", "Author", ContributorRole.Author)
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        var firstLine = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
        Assert.StartsWith("Author, First", firstLine);
        Assert.Contains("Second Author", firstLine);
        Assert.Contains("Third Author", firstLine);
        Assert.Contains(", and Fourth Author.", firstLine);
    }

    [Fact]
    public void FormatChicago_NonAuthorContributors_AreIgnored()
    {
        // Arrange
        var entry = CreateTestEntry(
            title: "Work with Contributors",
            publisher: "Publisher",
            year: 2023,
            style: CitationStyle.Chicago,
            contributors: new List<Contributor>
            {
                CreateContributor("John", "Doe", ContributorRole.Author),
                CreateContributor("Jane", "Editor", ContributorRole.Editor),
                CreateContributor("Jim", "Translator", ContributorRole.Translator)
            }
        );

        // Act
        var result = _formatter.FormatBibliography(new[] { entry });

        // Assert
        Assert.StartsWith("Doe, John.", result);
        Assert.DoesNotContain("Editor", result);
        Assert.DoesNotContain("Translator", result);
    }

    }
}