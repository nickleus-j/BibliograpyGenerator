using System;
using System.Collections.Generic;
using System.Text;

namespace Bibliography.Lib.Models
{
    public enum CitationStyle { APA, MLA, Chicago, Harvard, IEEE }
    public enum SourceType { Book, Journal, Website, Report }
    public enum ContributorRole { Author, Editor, Translator, Director }
    public enum AuthorCitationFormat
    {
        FullName,                  // John Smith
        LastNameFirst,             // Smith, John
        LastNameFirstInitial,      // Smith, J.
        LastNameFirstInitials      // Smith, J. Q.
    }

}
