using System.Collections.ObjectModel;
using Bibliography.Lib.Formatters;
using Bibliography.Lib.Models;
using Bibliography.Lib.Parsers;
using Bibluiography.GoogleBooks.Lib;

namespace Bibliography.Maui;

public partial class MainPage : ContentPage
{
  private ObservableCollection<BibliographyEntry> _bibliographyEntries;
    public ObservableCollection<BibliographyEntry> BibliographyEntries {
        get { return _bibliographyEntries; }
    }
    public MainPage()
    {
        _bibliographyEntries = new ObservableCollection<BibliographyEntry>();
        InitializeComponent();
        
        BibliographyGrid.ItemsSource = BibliographyEntries;
    }

    private void OnParseClicked(object sender, EventArgs e)
    {
        var bibTexInput = BibTexInput.Text;

        if (string.IsNullOrWhiteSpace(bibTexInput))
        {
            DisplayAlert("Input Required", "Please paste BibTeX entries in the text area.", "OK");
            return;
        }

        try
        {
            ParseButton.IsEnabled = false;
            ParseButton.Text = "Parsing...";

            // Parse the BibTeX string
            var entries = ParseBibTeXString(bibTexInput);

            _bibliographyEntries.Clear();
            foreach (var entry in entries)
            {
                _bibliographyEntries.Add(entry);
            }

            // Update UI
            EntryCountLabel.Text = $"{_bibliographyEntries.Count} {(_bibliographyEntries.Count == 1 ? "entry" : "entries")}";
            BibliographyGrid.IsVisible = _bibliographyEntries.Any();
            if (BibliographyGrid.IsVisible)
            {
                DisplayAlert("Success", $"Parsed {_bibliographyEntries.Count} bibliography entries.", "OK");
            }
            else
            {
                DisplayAlert("No Entries", "No valid BibTeX entries were found.", "OK");
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Parse Error", $"Error parsing BibTeX: {ex.Message}", "OK");
        }
        finally
        {
            ParseButton.IsEnabled = true;
            ParseButton.Text = "Parse BibTeX";
        }
    }

    private string GenerateApa()
    {
        var formatter = BibliographyFormatter.GetInstance().FormatBibliography(_bibliographyEntries,CitationStyle.APA);
        return formatter;
    }
    private async Task<string> GenerateMla()
    {
        GoogleBooksClient client = new GoogleBooksClient(AppNameBox.Text,ApiKeyBox.Text);
        var entries=new List<BibliographyEntry>();
        var entry=await client.GetBookByIsbnAsync(isbnBox.Text);
        entries.Add(entry);
        var formatter = BibliographyFormatter.GetInstance().FormatBibliography(entries,CitationStyle.MLA);
        return formatter;
    }
    private void OnApaClicked(object sender, EventArgs e)
    {
        Apa.Text = GenerateApa();
    }
    private async void OnLookupClicked(object sender, EventArgs e)
    {
        (sender as Button).IsEnabled = false;

        try
        {
            // Call your actual async method that returns a Task
            Apa.Text = await GenerateMla();
        
        }
        catch (Exception ex)
        {
            // Handle any exceptions gracefully within the handler
            await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            // Ensure UI elements are re-enabled
            (sender as Button).IsEnabled = true;
        }
    }
    private void OnClearClicked(object sender, EventArgs e)
    {
        BibTexInput.Text = string.Empty;
        _bibliographyEntries.Clear();
        EntryCountLabel.Text = "0 entries";
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (_bibliographyEntries.Count == 0)
        {
            await DisplayAlert("No Data", "There are no entries to export.", "OK");
            return;
        }

        try
        {
            var csvContent = GenerateCSV();
            var fileName = $"bibliography_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            // Save to file
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csvContent);

            await DisplayAlert("Export Successful", $"Exported to {fileName}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export Error", $"Error exporting: {ex.Message}", "OK");
        }
    }

    private string GenerateCSV()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Title,Authors,Type,Publisher,Year,Journal,Volume,Issue,DOI,URL");

        foreach (var entry in _bibliographyEntries)
        {
            var authors = string.Join("; ", entry.Contributors.Select(c => $"{c.FirstName} {c.LastName}"));
            var year = entry.PublicationDate?.Year.ToString() ?? "";
            
            sb.AppendLine($"\"{entry.Title}\",\"{authors}\",\"{entry.SourceType}\",\"{entry.Publisher}\",\"{year}\",\"{entry.ContainerTitle}\",\"{entry.Volume}\",\"{entry.Issue}\",\"{entry.DigitalObjectIdentifier}\",\"{entry.Url}\"");
        }

        return sb.ToString();
    }

    // Include the parsing method from the previous response
    private IList<BibliographyEntry> ParseBibTeXString(string bibTexString)
    {
        BibTexParser parser = new BibTexParser();
        var entries = parser.ParseBibTexEntries(bibTexString);
        
        return entries;
    }

}