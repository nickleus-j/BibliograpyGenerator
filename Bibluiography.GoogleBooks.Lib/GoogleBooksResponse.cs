namespace Bibluiography.GoogleBooks.Lib;

public record GoogleBooksResponse(List<BookItem> Items);
public record BookItem(VolumeInfo VolumeInfo);
public record VolumeInfo(
    string Title, 
    List<string>? Authors, 
    string? Publisher, 
    string? PublishedDate
);