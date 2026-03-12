using System.Net.Http.Json;
using Bibliography.Lib.Models;
using Bibliography.Lib.Parsers;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace Bibluiography.GoogleBooks.Lib;
public class GoogleBooksClient
{
    private string ApplicationName ;
    private string ApiKey ;

    public GoogleBooksClient(string appName, string apiKey)
    {
        ApplicationName = appName;
        ApiKey = apiKey;
    }
    public async Task<Volume> SearchTitle(string isbn)
    {
        BooksService service = new BooksService(
            new BaseClientService.Initializer
            {
                ApplicationName = ApplicationName,
                ApiKey = ApiKey,
            });
        try
        {
               
            Volumes result = await service.Volumes.List(isbn).ExecuteAsync();
            if (result is { Items: not null }&&result.Items.Any())
            {
                var item = result.Items.FirstOrDefault();
                return item;
            }
        }
        catch { }
            
        return null;

    }
    private const string ApiUrl = "https://www.googleapis.com/books/v1/volumes";
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Requires api key and App Name from google books to retrieve needed Data
    /// </summary>
    /// <param name="isbn"></param>
    /// <returns></returns>
    public async Task<BibliographyEntry> GetBookByIsbnAsync(string isbn)
    {
        try
        {
            if (string.IsNullOrEmpty(ApiKey))
                throw new InvalidOperationException("API key not configured. Please set it in settings.");

            string url = $"{ApiUrl}?q=isbn:{isbn}&key={ApiKey}";
            _httpClient.Timeout = TimeSpan.FromSeconds(10);

            using HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(jsonContent);

            if (result?.Items == null || result.Items.Count == 0)
                throw new InvalidOperationException($"No book found with ISBN: {isbn}");

            var info = result.Items.FirstOrDefault().VolumeInfo;
            var authors = AuthorNameParser.ParseAuthors(info.Authors);

            return new BibliographyEntry
            {
                Title = info.Title,
                Publisher = info.Publisher,
                SourceType = SourceType.Book,
                Contributors = authors?.Select(a => new Contributor
                {
                    FirstName = a.FirstNames,
                    LastName = a.Surname
                }).ToList() ?? new(),
                PublicationDate = ParseDate(info.PublishedDate)
            };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to fetch book data: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unexpected error: {ex.Message}", ex);
        }
    }

    public async Task<BibliographyEntry?> SearchBookByIsbnAsync(string q)
    {
        var response= await SearchTitle(q);
        // 2. Handle 'No Results'
        if (response == null )
            return null;

        // 3. Map to your BibliographyEntry
        var info = response.VolumeInfo;
        
        return new BibliographyEntry
        {
            Title = info.Title,
            Publisher = info.Publisher,
            SourceType = SourceType.Book,
            // Example of mapping the authors list to your Contributors list
            Contributors = info.Authors?.Select(a => new Contributor { FirstName = a,LastName = a}).ToList() ?? new(),
            PublicationDate = ParseDate(info.PublishedDate)
        };
    }

    private PublicationDate ParseDate(string? dateRaw)
    {
        // Google returns YYYY-MM-DD or just YYYY
        if (int.TryParse(dateRaw?.Split('-')[0], out int year))
            return new PublicationDate { Year = year };
            
        return new PublicationDate { Year = DateTime.Now.Year };
    }
}