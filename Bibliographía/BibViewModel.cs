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
    public class BibViewModel : INotifyPropertyChanged
    {
        // === Properties ===
        private BibliographyEntry _currentEntry = new BibliographyEntry();
        public BibliographyEntry CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
        }
        public DateOnly CurrentPublishDate
        {
            get => CurrentEntry.PublicationDate!=null
                ? DateOnly.FromDateTime(new DateTime(CurrentEntry.PublicationDate.Year, CurrentEntry.PublicationDate.Month ?? 1, CurrentEntry.PublicationDate.Day ?? 1))
                : DateOnly.FromDateTime(DateTime.Now);
            set => CurrentEntry.PublicationDate = new PublicationDate { Year = value.Year, Month = value.Month, Day = value.Day };
        }
        public ObservableCollection<BibliographyEntry> Entries { get; set; }
            = new ObservableCollection<BibliographyEntry>();

        private string _generatedOutput;
        public string GeneratedOutput
        {
            get => _generatedOutput;
            set { _generatedOutput = value; OnPropertyChanged(); }
        }

        public Array SourceTypes => Enum.GetValues(typeof(SourceType));
        public Array CitationStyles => Enum.GetValues(typeof(CitationStyle));

        // === Commands ===
        public ICommand AddEntryCommand { get; }
        public ICommand UpdateEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }
        public ICommand AddContributorCommand { get; }
        public ICommand RemoveContributorCommand { get; }
        public ICommand GenerateTextCommand { get; }
        public ICommand GenerateBibTeXCommand { get; }

        // === Constructor ===
        public BibViewModel()
        {
            AddEntryCommand = new RelayCommand(AddEntry);
            UpdateEntryCommand = new RelayCommand(UpdateEntry, () => CurrentEntry != null);
            DeleteEntryCommand = new RelayCommand(DeleteEntry, () => CurrentEntry != null);
            AddContributorCommand = new RelayCommand(AddContributor);
            RemoveContributorCommand = new RelayCommand(RemoveContributor);
            GenerateTextCommand = new RelayCommand(GenerateText);
            GenerateBibTeXCommand = new RelayCommand(GenerateBibTeX);
        }

        // === CRUD Methods ===
        private void AddEntry()
        {
            Entries.Add(CurrentEntry);
            CitationStyle citationStyle = CurrentEntry.CitationStyle;
            CurrentEntry = new BibliographyEntry(); // reset form
            CurrentEntry.CitationStyle = citationStyle; // keep selected style
        }

        private void UpdateEntry()
        {
            // In a real app, you’d track selection and update accordingly
            OnPropertyChanged(nameof(Entries));
        }

        private void DeleteEntry()
        {
            Entries.Remove(CurrentEntry);
            CurrentEntry = new BibliographyEntry();
        }

        private void AddContributor()
        {
            CurrentEntry.Contributors.Add(new Contributor { LastName = "New",FirstName="Write", Role = ContributorRole.Author });
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
            GeneratedOutput = BibliographyFormatter.GetInstance().FormatBibliography(Entries, CurrentEntry.CitationStyle);
        }

        private void GenerateBibTeX()
        {
            GeneratedOutput = BibTexFormatter.GetInstance().ToBibTeX(Entries);
        }

        // === INotifyPropertyChanged ===
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
