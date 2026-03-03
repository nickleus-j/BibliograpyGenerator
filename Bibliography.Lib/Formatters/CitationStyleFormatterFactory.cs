using Bibliography.Lib.Models;

namespace Bibliography.Lib.Formatters;

public class CitationStyleFormatterFactory
{
    private static readonly Dictionary<CitationStyle, IBibliographyStyleFormatter> _formatters =
        new Dictionary<CitationStyle, IBibliographyStyleFormatter>
        {
            { CitationStyle.APA, new ApaBiblioFormatter() },
            { CitationStyle.MLA, new MlaBiblioFormatter() },
            { CitationStyle.Chicago, new ChicagoBiblioFormatter() },
            { CitationStyle.Harvard, new HarvardBiblioFormatter() },
            { CitationStyle.IEEE, new IeeeBiblioFormatter() },
        };

    public static IBibliographyStyleFormatter GetFormatter(CitationStyle style)
    {
        if (_formatters.TryGetValue(style, out var formatter))
        {
            return formatter;
        }

        throw new ArgumentException($"Unsupported citation style: {style}");
    }
}