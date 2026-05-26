using System.Collections.Generic;

namespace LanguageLearningApp.Models
{
    public class MultipleChoiceQuestion
    {
        public int Id { get; set; }
        public int SetId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int Difficulty { get; set; } = 1;
    }
}