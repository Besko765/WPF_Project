using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LanguageLearningApp.Data;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.ViewModels
{
    public class SetsViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<Set> Sets { get; } = LanguageLearningApp.Data.DataService.Sets;

		private Set? selectedSet;
		public Set? SelectedSet
		{
			get => selectedSet;
			set
			{
				selectedSet = value;
				OnPropertyChanged();
				if (selectedSet != null)
				{
					NameInput = selectedSet.Name;
					OgLangInput = selectedSet.OgLanguage;
					NewLangInput = selectedSet.NewLanguage;
					SelectedWord = null;
				}
			}
		}

		private Word? selectedWord;
		public Word? SelectedWord
		{
			get => selectedWord;
			set
			{
				selectedWord = value;
				OnPropertyChanged();
				if (selectedWord != null)
				{
					WordTextInput = selectedWord.Text;
					WordTranslationInput = selectedWord.Translation;
				}
				else
				{
					WordTextInput = string.Empty;
					WordTranslationInput = string.Empty;
				}
			}
		}

		private string wordTextInput = string.Empty;
		public string WordTextInput
		{
			get => wordTextInput;
			set { wordTextInput = value; OnPropertyChanged(); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
		}

		private string wordTranslationInput = string.Empty;
		public string WordTranslationInput
		{
			get => wordTranslationInput;
			set { wordTranslationInput = value; OnPropertyChanged(); System.Windows.Input.CommandManager.InvalidateRequerySuggested(); }
		}

		private string nameInput = string.Empty;
		public string NameInput
		{
			get => nameInput;
			set { nameInput = value; OnPropertyChanged(); }
		}

		private string ogLangInput = string.Empty;
		public string OgLangInput
		{
			get => ogLangInput;
			set { ogLangInput = value; OnPropertyChanged(); }
		}

		private string newLangInput = string.Empty;
		public string NewLangInput
		{
			get => newLangInput;
			set { newLangInput = value; OnPropertyChanged(); }
		}

		public ICommand AddCommand { get; }
		public ICommand UpdateCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand ClearCommand { get; }
		public ICommand AddWordCommand { get; }
		public ICommand UpdateWordCommand { get; }
		public ICommand DeleteWordCommand { get; }

		public SetsViewModel()
		{

			AddCommand = new RelayCommand(_ => AddSet(), _ => !string.IsNullOrWhiteSpace(NameInput));
			UpdateCommand = new RelayCommand(_ => UpdateSet(), _ => SelectedSet != null);
			DeleteCommand = new RelayCommand(_ => DeleteSet(), _ => SelectedSet != null);
			ClearCommand = new RelayCommand(_ => ClearInputs());

			AddWordCommand = new RelayCommand(_ => AddWord(), _ => SelectedSet != null && !string.IsNullOrWhiteSpace(WordTextInput));
			UpdateWordCommand = new RelayCommand(_ => UpdateWord(), _ => SelectedSet != null && SelectedWord != null);
			DeleteWordCommand = new RelayCommand(_ => DeleteWord(), _ => SelectedSet != null && SelectedWord != null);

			if (Sets.Any())
			{
				SelectedSet = Sets.First();
			}
		}

		private void AddSet()
		{
			var newSet = new Set
			{
				Id = (Sets.Any() ? Sets.Max(s => s.Id) : 0) + 1,
				Name = NameInput,
				OgLanguage = OgLangInput,
				NewLanguage = NewLangInput
			};
			Sets.Add(newSet);
			ClearInputs();
		}

		private void UpdateSet()
		{
			if (SelectedSet == null) return;

			var idx = Sets.IndexOf(SelectedSet);
			if (idx < 0) return;

			var updated = new Set
			{
				Id = SelectedSet.Id,
				Name = NameInput,
				OgLanguage = OgLangInput,
				NewLanguage = NewLangInput
			};

			Sets[idx] = updated;
			SelectedSet = updated;
		}

		private void DeleteSet()
		{
			if (SelectedSet == null) return;
			Sets.Remove(SelectedSet);
			ClearInputs();
		}

		private void AddWord()
		{
			if (SelectedSet == null) return;
			var words = SelectedSet.Words;
			var newId = (words.Any() ? words.Max(w => w.Id) : 0) + 1;
			var w = new Word { Id = newId, SetId = SelectedSet.Id, Text = WordTextInput, Translation = WordTranslationInput };
			words.Add(w);
			WordTextInput = string.Empty;
			WordTranslationInput = string.Empty;
		}

		private void UpdateWord()
		{
			if (SelectedSet == null || SelectedWord == null) return;
			var words = SelectedSet.Words;
			var idx = words.IndexOf(SelectedWord);
			if (idx < 0) return;
			var updated = new Word { Id = SelectedWord.Id, SetId = SelectedSet.Id, Text = WordTextInput, Translation = WordTranslationInput };
			words[idx] = updated;
			SelectedWord = updated;
		}

		private void DeleteWord()
		{
			if (SelectedSet == null || SelectedWord == null) return;
			SelectedSet.Words.Remove(SelectedWord);
			SelectedWord = null;
		}

		private void ClearInputs()
		{
			NameInput = string.Empty;
			OgLangInput = string.Empty;
			NewLangInput = string.Empty;
			SelectedSet = null;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}