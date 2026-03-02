using Bibliography.Lib.Formatters;
using Bibliography.Lib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
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
        // === Commands ===
        public ICommand AddEntryCommand { get; }
        public ICommand UpdateEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }
        public ICommand AddContributorCommand { get; }
        public ICommand RemoveContributorCommand { get; }
        public ICommand GenerateTextCommand { get; }
        public ICommand GenerateBibTeXCommand { get; }

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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
