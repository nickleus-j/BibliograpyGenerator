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

        public string FormatBibliography(IEnumerable<BibliographyEntry> entries, CitationStyle? style=null)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (!entries.Any()) return "No entries provided.";
            
            var formatter = CitationStyleFormatterFactory.GetFormatter(style==null?entries.First().CitationStyle:style.Value);
            return formatter.FormatBibliography(entries);
        }

        #endregion
    }
}
