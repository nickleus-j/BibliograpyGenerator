using Xunit;
using System.Collections.Generic;
using System.Linq;
using Bibliography.Lib.Models;
using Bibliography.Lib.Parsers;

namespace Bibliogrphy.Lib.Tests;
public class AuthorNameParserTests
{
    #region ParseName Tests

    [Fact]
    public void ParseName_WithNullInput_ReturnsAuthorWithNullFullName()
    {
        // Act
        var result = AuthorNameParser.ParseName(null);

        // Assert
        Assert.Null(result.FullName);
        Assert.Null(result.FirstNames);
        Assert.Null(result.Surname);
    }

    [Fact]
    public void ParseName_WithEmptyString_ReturnsAuthorWithEmptyFullName()
    {
        // Act
        var result = AuthorNameParser.ParseName(string.Empty);

        // Assert
        Assert.Empty(result.FullName);
        Assert.Null(result.FirstNames);
        Assert.Null(result.Surname);
    }

    [Fact]
    public void ParseName_WithWhitespaceOnly_ReturnsAuthorWithEmptyFullName()
    {
        // Act
        var result = AuthorNameParser.ParseName("   ");

        // Assert
        Assert.Empty(result.FullName.Trim());
    }

    [Fact]
    public void ParseName_WithSingleName_ParsesAsSurname()
    {
        // Arrange
        string fullName = "Smith";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("Smith", result.Surname);
        Assert.Empty(result.FirstNames);
        Assert.Equal("Smith", result.FullName);
    }

    [Fact]
    public void ParseName_WithTwoNames_ParsesAsFirstNameAndSurname()
    {
        // Arrange
        string fullName = "John Smith";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("John", result.FirstNames);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("John Smith", result.FullName);
    }

    [Fact]
    public void ParseName_WithThreeNames_ParsesLastAsSurnameAndRestAsFirstNames()
    {
        // Arrange
        string fullName = "John Michael Smith";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("John Michael", result.FirstNames);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("John Michael Smith", result.FullName);
    }

    [Fact]
    public void ParseName_WithMultipleNames_ParsesLastAsSurnameAndRestAsFirstNames()
    {
        // Arrange
        string fullName = "Jean Paul Marie Sartre";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("Jean Paul Marie", result.FirstNames);
        Assert.Equal("Sartre", result.Surname);
    }

    [Fact]
    public void ParseName_WithCommaFormat_ParsesSurnameBeforeComma()
    {
        // Arrange
        string fullName = "Smith, John";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("John", result.FirstNames);
        Assert.Equal("Smith, John", result.FullName);
    }

    [Fact]
    public void ParseName_WithCommaFormatAndMultipleFirstNames_ParsesCorrectly()
    {
        // Arrange
        string fullName = "Smith, John Michael";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("John Michael", result.FirstNames);
    }

    [Fact]
    public void ParseName_WithCommaFormatAndNoFirstName_ParsesOnlySurname()
    {
        // Arrange
        string fullName = "Smith,";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("Smith", result.Surname);
        Assert.Empty(result.FirstNames);
    }

    [Fact]
    public void ParseName_WithLeadingAndTrailingWhitespace_TrimsCorrectly()
    {
        // Arrange
        string fullName = "  John Smith  ";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("John", result.FirstNames);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("John Smith", result.FullName);
    }

    [Fact]
    public void ParseName_WithMiddleInitial_ParsesCorrectly()
    {
        // Arrange
        string fullName = "John Q. Smith";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("John Q.", result.FirstNames);
        Assert.Equal("Smith", result.Surname);
    }

    [Fact]
    public void ParseName_WithMultipleSpaces_RemovesEmptyEntries()
    {
        // Arrange
        string fullName = "John    Michael    Smith";

        // Act
        var result = AuthorNameParser.ParseName(fullName);

        // Assert
        Assert.Equal("John Michael", result.FirstNames);
        Assert.Equal("Smith", result.Surname);
    }

    #endregion

    #region ParseAuthors Tests

    [Fact]
    public void ParseAuthors_WithNullList_ReturnsEmptyList()
    {
        // Act
        var result = AuthorNameParser.ParseAuthors(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseAuthors_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var authorNames = new List<string>();

        // Act
        var result = AuthorNameParser.ParseAuthors(authorNames);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseAuthors_WithSingleAuthor_ReturnsSingleParsedAuthor()
    {
        // Arrange
        var authorNames = new List<string> { "John Smith" };

        // Act
        var result = AuthorNameParser.ParseAuthors(authorNames);

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstNames);
        Assert.Equal("Smith", result[0].Surname);
    }

    [Fact]
    public void ParseAuthors_WithMultipleAuthors_ReturnsAllParsedAuthors()
    {
        // Arrange
        var authorNames = new List<string>
        {
            "John Smith",
            "Jane Doe",
            "Robert Johnson"
        };

        // Act
        var result = AuthorNameParser.ParseAuthors(authorNames);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("John", result[0].FirstNames);
        Assert.Equal("Smith", result[0].Surname);
        Assert.Equal("Jane", result[1].FirstNames);
        Assert.Equal("Doe", result[1].Surname);
        Assert.Equal("Robert", result[2].FirstNames);
        Assert.Equal("Johnson", result[2].Surname);
    }

    [Fact]
    public void ParseAuthors_WithMixedFormats_ParsesAllCorrectly()
    {
        // Arrange
        var authorNames = new List<string>
        {
            "John Smith",
            "Doe, Jane",
            "Robert Michael Johnson"
        };

        // Act
        var result = AuthorNameParser.ParseAuthors(authorNames);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Smith", result[0].Surname);
        Assert.Equal("Doe", result[1].Surname);
        Assert.Equal("Jane", result[1].FirstNames);
        Assert.Equal("Johnson", result[2].Surname);
    }

    #endregion

    #region FormatForCitation Tests

    [Fact]
    public void FormatForCitation_WithNullSurname_ReturnsFullName()
    {
        // Arrange
        var author = new AuthorName { FullName = "Unknown", FirstNames = "John", Surname = null };

        // Act
        var result = AuthorNameParser.FormatForCitation(author);

        // Assert
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void FormatForCitation_WithEmptySurname_ReturnsFullName()
    {
        // Arrange
        var author = new AuthorName { FullName = "Unknown", FirstNames = "John", Surname = string.Empty };

        // Act
        var result = AuthorNameParser.FormatForCitation(author);

        // Assert
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void FormatForCitation_WithFullNameFormat_ReturnsFullName()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Smith", FirstNames = "John", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.FullName);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstFormat_ReturnsLastNameFirst()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Smith", FirstNames = "John", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirst);

        // Assert
        Assert.Equal("Smith, John", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstFormat_AndNoFirstNames_ReturnsSurnameOnly()
    {
        // Arrange
        var author = new AuthorName { FullName = "Smith", FirstNames = string.Empty, Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirst);

        // Assert
        Assert.Equal("Smith", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstInitialFormat_ReturnsSurnameAndInitial()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Smith", FirstNames = "John", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitial);

        // Assert
        Assert.Equal("Smith, J.", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstInitialFormat_AndMultipleFirstNames_ReturnsFirstInitialOnly()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Michael Smith", FirstNames = "John Michael", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitial);

        // Assert
        Assert.Equal("Smith, J.", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstInitialFormat_AndNoFirstNames_ReturnsSurnameOnly()
    {
        // Arrange
        var author = new AuthorName { FullName = "Smith", FirstNames = string.Empty, Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitial);

        // Assert
        Assert.Equal("Smith", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstInitialsFormat_ReturnsAllInitials()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Michael Smith", FirstNames = "John Michael", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitials);

        // Assert
        Assert.Equal("Smith, J. M.", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstInitialsFormat_AndSingleFirstName_ReturnsSingleInitial()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Smith", FirstNames = "John", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitials);

        // Assert
        Assert.Equal("Smith, J.", result);
    }

    [Fact]
    public void FormatForCitation_WithLastNameFirstInitialsFormat_AndNoFirstNames_ReturnsSurnameOnly()
    {
        // Arrange
        var author = new AuthorName { FullName = "Smith", FirstNames = string.Empty, Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitials);

        // Assert
        Assert.Equal("Smith", result);
    }

    [Fact]
    public void FormatForCitation_WithDefaultFormat_UsesLastNameFirstInitial()
    {
        // Arrange
        var author = new AuthorName { FullName = "John Smith", FirstNames = "John", Surname = "Smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author);

        // Assert
        Assert.Equal("Smith, J.", result);
    }

    [Fact]
    public void FormatForCitation_WithLowercaseFirstName_ReturnsUppercaseInitial()
    {
        // Arrange
        var author = new AuthorName { FullName = "john smith", FirstNames = "john", Surname = "smith" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitial);

        // Assert
        Assert.Equal("smith, J.", result);
    }

    [Fact]
    public void FormatForCitation_WithMultipleInitials_HandlesWhitespaceCorrectly()
    {
        // Arrange
        var author = new AuthorName { FullName = "J. R. R. Tolkien", FirstNames = "J. R. R.", Surname = "Tolkien" };

        // Act
        var result = AuthorNameParser.FormatForCitation(author, AuthorCitationFormat.LastNameFirstInitials);

        // Assert
        Assert.Equal("Tolkien, J. R. R.", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ParseName_AndFormatForCitation_Integration_SimpleFormat()
    {
        // Arrange
        string fullName = "John Smith";

        // Act
        var parsed = AuthorNameParser.ParseName(fullName);
        var formatted = AuthorNameParser.FormatForCitation(parsed, AuthorCitationFormat.LastNameFirstInitial);

        // Assert
        Assert.Equal("Smith, J.", formatted);
    }

    [Fact]
    public void ParseName_AndFormatForCitation_Integration_ComplexFormat()
    {
        // Arrange
        string fullName = "Jean Paul Sartre";

        // Act
        var parsed = AuthorNameParser.ParseName(fullName);
        var formatted = AuthorNameParser.FormatForCitation(parsed, AuthorCitationFormat.LastNameFirstInitials);

        // Assert
        Assert.Equal("Sartre, J. P.", formatted);
    }

    [Fact]
    public void ParseName_AndFormatForCitation_Integration_CommaFormat()
    {
        // Arrange
        string fullName = "Doe, Jane Marie";

        // Act
        var parsed = AuthorNameParser.ParseName(fullName);
        var formatted = AuthorNameParser.FormatForCitation(parsed, AuthorCitationFormat.LastNameFirst);

        // Assert
        Assert.Equal("Doe, Jane Marie", formatted);
    }

    [Fact]
    public void ParseAuthors_AndFormatMultiple_Integration()
    {
        // Arrange
        var authorNames = new List<string>
        {
            "John Smith",
            "Jane Doe",
            "Robert Johnson"
        };

        // Act
        var parsed = AuthorNameParser.ParseAuthors(authorNames);
        var formatted = parsed.Select(a => AuthorNameParser.FormatForCitation(a, AuthorCitationFormat.LastNameFirstInitial)).ToList();

        // Assert
        Assert.Equal(3, formatted.Count);
        Assert.Equal("Smith, J.", formatted[0]);
        Assert.Equal("Doe, J.", formatted[1]);
        Assert.Equal("Johnson, R.", formatted[2]);
    }

    #endregion
}
