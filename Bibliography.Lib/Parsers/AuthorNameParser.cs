using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bibliography.Lib.Models;

namespace Bibliography.Lib.Parsers;
public class AuthorNameParser
{
    /// <summary>
    /// Parses an author's full name into surname and first names.
    /// Handles various name formats: "John Smith", "Smith, John", "John Q. Smith", etc.
    /// </summary>
    public static AuthorName ParseName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return new AuthorName { FullName = fullName };

        fullName = fullName.Trim();
        var author = new AuthorName { FullName = fullName };

        // Handle "Surname, FirstName" format
        if (fullName.Contains(","))
        {
            var parts = fullName.Split(',');
            author.Surname = parts[0].Trim();
            author.FirstNames = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            return author;
        }

        // Handle single name (no space)
        if (!fullName.Contains(" "))
        {
            author.Surname = fullName;
            author.FirstNames = string.Empty;
            return author;
        }

        // Handle multi-part names
        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length == 2)
        {
            // Simple "FirstName Surname" format
            author.FirstNames = nameParts[0];
            author.Surname = nameParts[1];
        }
        else if (nameParts.Length > 2)
        {
            // Complex names: "FirstName MiddleName Surname" or variations
            // Assume last part is surname, everything else is first/middle names
            author.Surname = nameParts[nameParts.Length - 1];
            author.FirstNames = string.Join(" ", nameParts.Take(nameParts.Length - 1));
        }

        return author;
    }

    /// <summary>
    /// Parses multiple author names and returns a list of parsed names.
    /// </summary>
    public static List<AuthorName> ParseAuthors(List<string> authorNames)
    {
        return authorNames?.Select(ParseName).ToList() ?? new List<AuthorName>();
    }
    /// <summary>
    /// Formats author names for citation purposes (e.g., "Smith, J." or "Smith, John").
    /// </summary>
    public static string FormatForCitation(AuthorName author, AuthorCitationFormat format = AuthorCitationFormat.LastNameFirstInitial)
    {
        if (string.IsNullOrWhiteSpace(author.Surname))
            return author.FullName;

        return format switch
        {
            AuthorCitationFormat.LastNameFirst => 
                $"{author.Surname}, {author.FirstNames}".TrimEnd(',', ' '),
            
            AuthorCitationFormat.LastNameFirstInitial => 
                FormatFirstInitial(author.Surname, author.FirstNames),
            
            AuthorCitationFormat.LastNameFirstInitials => 
                FormatFirstInitials(author.Surname, author.FirstNames),
            
            AuthorCitationFormat.FullName => 
                author.FullName,
            
            _ => author.FullName
        };
    }

    private static string FormatFirstInitial(string surname, string firstNames)
    {
        if (string.IsNullOrWhiteSpace(firstNames))
            return surname;

        string initial = firstNames.Split(' ')[0][0].ToString().ToUpper();
        return $"{surname}, {initial}.";
    }

    private static string FormatFirstInitials(string surname, string firstNames)
    {
        if (string.IsNullOrWhiteSpace(firstNames))
            return surname;

        var initials = string.Join(". ", 
            firstNames.Split(' ')
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n[0].ToString().ToUpper()));

        return $"{surname}, {initials}.";
    }
}

public class AuthorName
{
    public string FullName { get; set; }
    public string FirstNames { get; set; }
    public string Surname { get; set; }

    public override string ToString()
    {
        return $"{FirstNames} {Surname}".Trim();
    }
}

