using System.Windows;
using LanguageLearningApp.Models;
using System.Linq;
using System.Collections.ObjectModel;

namespace LanguageLearningApp.Views
{
    public partial class SetEditorWindow : Window
    {
        private Set? originalSet;
        private Set editingSet;
        private bool isReadOnly;
        private bool isAddMode;

        public Set? EditedSet { get; private set; }

        public SetEditorWindow(Set set, bool isEdit = true, bool readOnly = false)
        {
            InitializeComponent();

            isReadOnly = readOnly;

            if (isEdit)
            {
                originalSet = set;
                editingSet = CloneSet(set);
                isAddMode = false;
            }
            else
            {
                originalSet = null;
                editingSet = CloneSet(set);
                isAddMode = true;
            }

            // populate UI from editingSet
            NameInput.Text = editingSet.Name;
            OgLangInput.Text = editingSet.OgLanguage;
            NewLangInput.Text = editingSet.NewLanguage;

            WordsList.ItemsSource = editingSet.Words;

            ApplyMode();
        }

        private void ApplyMode()
        {
            if (isReadOnly)
            {
                NameInput.IsReadOnly = true;
                OgLangInput.IsReadOnly = true;
                NewLangInput.IsReadOnly = true;
                WordText.IsReadOnly = true;
                WordTranslation.IsReadOnly = true;

                AddWordBtn.IsEnabled = false;
                UpdateWordBtn.IsEnabled = false;
                DeleteWordBtn.IsEnabled = false;

                SaveBtn.Visibility = Visibility.Collapsed;
                CancelBtn.Visibility = Visibility.Collapsed;
                CloseBtn.Visibility = Visibility.Visible;
            }
            else
            {
                NameInput.IsReadOnly = false;
                OgLangInput.IsReadOnly = false;
                NewLangInput.IsReadOnly = false;
                WordText.IsReadOnly = false;
                WordTranslation.IsReadOnly = false;

                AddWordBtn.IsEnabled = true;
                UpdateWordBtn.IsEnabled = true;
                DeleteWordBtn.IsEnabled = true;

                SaveBtn.Visibility = Visibility.Visible;
                CancelBtn.Visibility = Visibility.Visible;
                CloseBtn.Visibility = Visibility.Collapsed;
            }
        }

        private Set CloneSet(Set s)
        {
            return new Set
            {
                Id = s.Id,
                Name = s.Name,
                OgLanguage = s.OgLanguage,
                NewLanguage = s.NewLanguage,
                Words = new ObservableCollection<Word>(s.Words.Select(w => new Word { Id = w.Id, SetId = w.SetId, Text = w.Text, Translation = w.Translation }))
            };
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (isReadOnly) return;
            var text = WordText.Text?.Trim();
            var trans = WordTranslation.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;
            var newId = (editingSet.Words.Any() ? editingSet.Words.Max(w => w.Id) : 0) + 1;
            editingSet.Words.Add(new Word { Id = newId, SetId = editingSet.Id, Text = text, Translation = trans ?? string.Empty });
            WordText.Text = string.Empty;
            WordTranslation.Text = string.Empty;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (isReadOnly) return;
            if (WordsList.SelectedItem is Word w)
            {
                int idx = editingSet.Words.IndexOf(w);
                if (idx < 0) return;
                editingSet.Words[idx] = new Word { Id = w.Id, SetId = editingSet.Id, Text = WordText.Text ?? string.Empty, Translation = WordTranslation.Text ?? string.Empty };
                WordsList.SelectedItem = editingSet.Words[idx];
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (isReadOnly) return;
            if (WordsList.SelectedItem is Word w)
            {
                editingSet.Words.Remove(w);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // copy UI fields to editingSet
            editingSet.Name = NameInput.Text ?? string.Empty;
            editingSet.OgLanguage = OgLangInput.Text ?? string.Empty;
            editingSet.NewLanguage = NewLangInput.Text ?? string.Empty;

            if (originalSet != null && !isAddMode)
            {
                // apply changes back to originalSet
                originalSet.Name = editingSet.Name;
                originalSet.OgLanguage = editingSet.OgLanguage;
                originalSet.NewLanguage = editingSet.NewLanguage;

                // replace words collection contents
                originalSet.Words.Clear();
                foreach (var w in editingSet.Words)
                {
                    originalSet.Words.Add(new Word { Id = w.Id, SetId = originalSet.Id, Text = w.Text, Translation = w.Translation });
                }

                this.DialogResult = true;
            }
            else
            {
                // return the newly created set
                EditedSet = new Set
                {
                    Id = editingSet.Id,
                    Name = editingSet.Name,
                    OgLanguage = editingSet.OgLanguage,
                    NewLanguage = editingSet.NewLanguage,
                    Words = new ObservableCollection<Word>(editingSet.Words.Select(w => new Word { Id = w.Id, SetId = editingSet.Id, Text = w.Text, Translation = w.Translation }))
                };

                this.DialogResult = true;
            }

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}