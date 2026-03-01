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
    public class BibliographyEntryViewModel : INotifyPropertyChanged
    {
        public BibliographyEntry _entry;
        public ICommand AddContributorCommand { get; }
        public ICommand RemoveContributorCommand { get; }

        public BibliographyEntryViewModel(BibliographyEntry entry)
        {
            _entry = entry;

            Contributors = new ObservableCollection<Contributor>(_entry.Contributors);

            AddContributorCommand = new RelayObjectCommand(_ =>
            {
                var c = new Contributor { FirstName = "", LastName = "", Role = ContributorRole.Author };
                Contributors.Add(c);
                _entry.Contributors.Add(c);
            });

            RemoveContributorCommand = new RelayObjectCommand(c =>
            {
                if (c is Contributor contributor)
                {
                    Contributors.Remove(contributor);
                    _entry.Contributors.Remove(contributor);
                }
                    
            });
        }
        // ------------------------------
        // ENUMS
        // ------------------------------

        public CitationStyle CitationStyle
        {
            get => _entry.CitationStyle;
            set { _entry.CitationStyle = value; OnPropertyChanged(); }
        }

        public SourceType SourceType
        {
            get => _entry.SourceType;
            set { _entry.SourceType = value; OnPropertyChanged(); }
        }

        // ------------------------------
        // TEXT FIELDS
        // ------------------------------
        public string Title
        {
            get => _entry.Title;
            set { _entry.Title = value; OnPropertyChanged(); }
        }

        public string? Publisher
        {
            get => _entry.Publisher;
            set { _entry.Publisher = value; OnPropertyChanged(); }
        }

        public string? DigitalObjectIdentifier
        {
            get => _entry.DigitalObjectIdentifier;
            set { _entry.DigitalObjectIdentifier = value; OnPropertyChanged(); }
        }

        public string? Url
        {
            get => _entry.Url;
            set { _entry.Url = value; OnPropertyChanged(); }
        }

        public string? ContainerTitle
        {
            get => _entry.ContainerTitle;
            set { _entry.ContainerTitle = value; OnPropertyChanged(); }
        }

        public string? Volume
        {
            get => _entry.Volume;
            set { _entry.Volume = value; OnPropertyChanged(); }
        }

        public string? Issue
        {
            get => _entry.Issue;
            set { _entry.Issue = value; OnPropertyChanged(); }
        }

        public string? Pages
        {
            get => _entry.Pages;
            set { _entry.Pages = value; OnPropertyChanged(); }
        }

        // ------------------------------
        // CONTRIBUTORS
        // ------------------------------

        public ObservableCollection<Contributor> Contributors { get; }

        // ------------------------------
        // PUBLICATION DATE
        // ------------------------------

        public int? PublicationYear
        {
            get => _entry.PublicationDate?.Year;
            set
            {
                EnsurePublicationDate();
                _entry.PublicationDate!.Year = value ?? DateTime.Now.Year;
                OnPropertyChanged();
            }
        }

        public int? PublicationMonth
        {
            get => _entry.PublicationDate?.Month;
            set
            {
                EnsurePublicationDate();
                _entry.PublicationDate!.Month = value ?? 1;
                OnPropertyChanged();
            }
        }

        public int? PublicationDay
        {
            get => _entry.PublicationDate?.Day;
            set
            {
                EnsurePublicationDate();
                _entry.PublicationDate!.Day = value ?? 1;
                OnPropertyChanged();
            }
        }

        private void EnsurePublicationDate()
        {
            if (_entry.PublicationDate == null)
                _entry.PublicationDate = new PublicationDate
                {
                    Year = DateTime.Now.Year,
                    Month = 1,
                    Day = 1
                };
        }

        // ------------------------------
        // ACCESS DATE
        // ------------------------------

        public DateOnly? AccessDate
        {
            get => _entry.AccessDate;
            set { _entry.AccessDate = value; OnPropertyChanged(); }
        }

        // ------------------------------
        // INotifyPropertyChanged
        // ------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    //Enum Providers (for ComboBox binding)
    public static class CitationStyleEnumValues
    {
        public static Array All => Enum.GetValues(typeof(CitationStyle));
    }

    public static class SourceTypeEnumValues
    {
        public static Array All => Enum.GetValues(typeof(SourceType));
    }

    public static class ContributorRoleEnumValues
    {
        public static Array All => Enum.GetValues(typeof(ContributorRole));
    }
}
