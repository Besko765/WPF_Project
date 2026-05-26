using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageLearningApp.Models
{
	public class Word
	{
		public int Id { get; set; }
		public int SetId { get; set; }
		public string Text { get; set; } = string.Empty;
		public string Translation { get; set; } = string.Empty; 
		public string Example { get; set; } = string.Empty;
		public int Difficulty { get; set; } = 1;
	}
}
