using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;
using LanguageLearningApp.Models;

namespace LanguageLearningApp.Data
{
	public static class DataService
	{
		public static List<Set> GetSets()
		{
			return new List<Set>
		{
			new Set { Id = 1, Name = "Podstawy", OgLanguage = "PL", NewLanguage = "EN" },
			new Set { Id = 2, Name = "Jedzenie", OgLanguage = "PL", NewLanguage = "EN" },
			new Set { Id = 3, Name = "Podróże", OgLanguage = "PL", NewLanguage = "DE" }
		};
		}
	}
}
