using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Specialized;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Data
{
	public static class DataService
	{
		private static readonly string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LanguageLearningApp");
		private static readonly string dataFile = Path.Combine(dataDir, "sets.json");

		private static System.Collections.ObjectModel.ObservableCollection<Set>? sets;

		public static System.Collections.ObjectModel.ObservableCollection<Set> Sets
		{
			get
			{
				if (sets == null)
				{
					LoadSets();
				}

				return sets!;
			}
		}

		public static System.Collections.ObjectModel.ObservableCollection<Set> GetSets() => Sets;

		private static void LoadSets()
		{
			try
			{
				if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

				if (File.Exists(dataFile))
				{
					var json = File.ReadAllText(dataFile, Encoding.UTF8);
					var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
					var list = JsonSerializer.Deserialize<List<Set>>(json, opts) ?? new List<Set>();
					sets = new System.Collections.ObjectModel.ObservableCollection<Set>(list);
				}
				else
				{
					// seed default data
					sets = new System.Collections.ObjectModel.ObservableCollection<Set>
					{
						new Set {
							Id = 1,
							Name = "Podstawy",
							OgLanguage = "PL",
							NewLanguage = "EN",
							Words = new System.Collections.ObjectModel.ObservableCollection<Word>
							{
								new Word { Id = 1, SetId = 1, Text = "kot", Translation = "cat" },
								new Word { Id = 2, SetId = 1, Text = "pies", Translation = "dog" },
								new Word { Id = 3, SetId = 1, Text = "dom", Translation = "house" }
							}
						},
						new Set {
							Id = 2,
							Name = "Jedzenie",
							OgLanguage = "PL",
							NewLanguage = "EN",
							Words = new System.Collections.ObjectModel.ObservableCollection<Word>
							{
								new Word { Id = 4, SetId = 2, Text = "chleb", Translation = "bread" },
								new Word { Id = 5, SetId = 2, Text = "ser", Translation = "cheese" },
								new Word { Id = 6, SetId = 2, Text = "jabłko", Translation = "apple" }
							}
						},
						new Set {
							Id = 3,
							Name = "Podróże",
							OgLanguage = "PL",
							NewLanguage = "DE",
							Words = new System.Collections.ObjectModel.ObservableCollection<Word>
							{
								new Word { Id = 7, SetId = 3, Text = "samolot", Translation = "Flugzeug" },
								new Word { Id = 8, SetId = 3, Text = "bilet", Translation = "Fahrkarte" },
								new Word { Id = 9, SetId = 3, Text = "hotel", Translation = "Hotel" }
							}
						}
					};

					SaveSets();
				}

				// subscribe to changes to auto-save
				SubscribeCollectionChanges();
			}
			catch (Exception ex)
			{
				// fallback: init empty collection
				sets = new System.Collections.ObjectModel.ObservableCollection<Set>();
				System.Diagnostics.Debug.WriteLine($"DataService.LoadSets error: {ex}");
			}
		}

		private static void SubscribeCollectionChanges()
		{
			if (sets == null) return;

			sets.CollectionChanged += Sets_CollectionChanged;
			foreach (var s in sets)
			{
				if (s.Words != null)
					s.Words.CollectionChanged += Words_CollectionChanged;
			}
		}

		private static void Sets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			// subscribe/unsubscribe for word-level changes
			if (e.OldItems != null)
			{
				foreach (Set old in e.OldItems)
				{
					if (old.Words != null)
						old.Words.CollectionChanged -= Words_CollectionChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (Set ns in e.NewItems)
				{
					if (ns.Words != null)
						ns.Words.CollectionChanged += Words_CollectionChanged;
				}
			}

			SaveSets();
		}

		private static void Words_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			SaveSets();
		}

		public static void SaveSets()
		{
			try
			{
				if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
				var opts = new JsonSerializerOptions { WriteIndented = true };
				var json = JsonSerializer.Serialize(Sets, opts);
				File.WriteAllText(dataFile, json, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"DataService.SaveSets error: {ex}");
			}
		}
	}
}

// Uwaga: DataService obecnie zapisuje i odczytuje z JSON w AppData. Modele gier takie jak Flashcard,
// MultipleChoiceQuestion czy MatchPair są zdefiniowane w folderze Models i są gotowe do użycia.
