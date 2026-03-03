using Bibliography.Lib.Formatters;
using Bibliography.Lib.Models;
using Bibliography.Lib.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;

namespace Bibliographía
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private BibliographyEntryViewModel _currentEntry;
        public BibliographyEntryViewModel CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
        }
        public ObservableCollection<BibliographyEntryViewModel> Entries { get; }
            = new ObservableCollection<BibliographyEntryViewModel>();
        private string _generatedOutput;
        public string GeneratedOutput
        {
            get => _generatedOutput;
            set { _generatedOutput = value; OnPropertyChanged(); }
        }
        private CitationStyle _citationStyle= CitationStyle.APA;
        public CitationStyle CitationStyle
        {
            get => _citationStyle;
            set { _citationStyle = value; OnPropertyChanged(); }
        }
        private string _bibTexForParse;
        public string BibTexForParse { get => _bibTexForParse; set { _bibTexForParse = value;OnPropertyChanged(); } }
        // === Commands ===
        public ICommand AddEntryCommand { get; }
        public ICommand UpdateEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }
        public ICommand AddContributorCommand { get; }
        public ICommand RemoveContributorCommand { get; }
        public ICommand GenerateTextCommand { get; }
        public ICommand GenerateBibTeXCommand { get; }
        public ICommand ParseBibTeXCommand { get; }

        // === Constructor ===
        public MainWindowViewModel()
        {
            // Sample items
            Entries.Add(new BibliographyEntryViewModel(new BibliographyEntry { Title = "Example 1",Publisher="Pub",SourceType=SourceType.Book }));
            AddEntryCommand = new RelayCommand(AddEntry);
            UpdateEntryCommand = new RelayCommand(UpdateEntry, () => CurrentEntry != null);
            DeleteEntryCommand = new RelayCommand(DeleteEntry, () => CurrentEntry != null);
            AddContributorCommand = new RelayCommand(AddContributor);
            RemoveContributorCommand = new RelayCommand(RemoveContributor);
            GenerateTextCommand = new RelayCommand(GenerateText);
            GenerateBibTeXCommand = new RelayCommand(GenerateBibTeX);
            ParseBibTeXCommand = new RelayCommand(ParseBibTex);
            CurrentEntry=Entries[0]; // Load first entry for editing
        }
        // === CRUD Methods ===
        private void AddEntry()
        {
            CitationStyle citationStyle = CurrentEntry.CitationStyle;
            CurrentEntry = new BibliographyEntryViewModel(new BibliographyEntry { Title = "Example 1", Publisher = "Pub", SourceType = SourceType.Book }); // reset form
            CurrentEntry.CitationStyle = citationStyle; // keep selected style
            Entries.Add(CurrentEntry);
            OnPropertyChanged(nameof(Entries));
        }

        private void UpdateEntry()
        {
            // In a real app, you’d track selection and update accordingly
            OnPropertyChanged(nameof(Entries));
        }

        private void DeleteEntry()
        {
            Entries.Remove(CurrentEntry);
            CurrentEntry = new BibliographyEntryViewModel(new BibliographyEntry { Title = "Example 1", Publisher = "Pub", SourceType = SourceType.Book }); // reset form
        }

        private void AddContributor()
        {
            CurrentEntry.Contributors.Add(new Contributor { LastName = "New", FirstName = "Write", Role = ContributorRole.Author });
            OnPropertyChanged(nameof(CurrentEntry));
        }

        private void RemoveContributor()
        {
            if (CurrentEntry.Contributors.Any())
                CurrentEntry.Contributors.RemoveAt(CurrentEntry.Contributors.Count - 1);
            OnPropertyChanged(nameof(CurrentEntry));
        }
        // === Generation Methods ===
        private void GenerateText()
        {
            GeneratedOutput = BibliographyFormatter.GetInstance().FormatBibliography(Entries.Select(e => e._entry), CitationStyle);
        }

        private void GenerateBibTeX()
        {
            GeneratedOutput = BibTexFormatter.GetInstance().ToBibTeX(Entries.Select(e=>e._entry));
        }
        private void ParseBibTex()
        {
            BibTexParser parser = new BibTexParser();
            IList<BibliographyEntry> parsedEntries = parser.ParseBibTexEntries(BibTexForParse);
            Entries.Clear();
            foreach (var item in parsedEntries)
            {
                Entries.Add(new BibliographyEntryViewModel(item));
            }
            GeneratedOutput = parsedEntries.Count.ToString() + " entries parsed.";
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
