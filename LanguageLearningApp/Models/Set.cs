using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageLearningApp.Models
{
	public class Set
	{
     public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string OgLanguage { get; set; } = string.Empty;
		public string NewLanguage { get; set; } = string.Empty;
		public System.Collections.ObjectModel.ObservableCollection<Word> Words { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Word>();
	}
}
