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
		public ObservableCollection<Set> Sets { get; } = new ObservableCollection<Set>();

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
				}
			}
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

		public SetsViewModel()
		{
			foreach (var s in DataService.GetSets())
				Sets.Add(s);

			AddCommand = new RelayCommand(_ => AddSet(), _ => !string.IsNullOrWhiteSpace(NameInput));
			UpdateCommand = new RelayCommand(_ => UpdateSet(), _ => SelectedSet != null);
			DeleteCommand = new RelayCommand(_ => DeleteSet(), _ => SelectedSet != null);
			ClearCommand = new RelayCommand(_ => ClearInputs());
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