namespace LanguageLearningApp.Models
{
    public class Flashcard
    {
        public int Id { get; set; }
        public int SetId { get; set; }
        public string Front { get; set; } = string.Empty; // question or original word
        public string Back { get; set; } = string.Empty;  // translation or answer
        public string Example { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 1;
    }
}