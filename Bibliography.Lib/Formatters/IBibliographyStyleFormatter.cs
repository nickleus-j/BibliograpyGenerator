using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public interface IBibliographyStyleFormatter
{
    string FormatBibliography(IEnumerable<BibliographyEntry> entries);
}