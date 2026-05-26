namespace LanguageLearningApp.Models
{
    public class MatchPair
    {
        public int Id { get; set; }
        public int SetId { get; set; }
        public string Left { get; set; } = string.Empty;
        public string Right { get; set; } = string.Empty;
    }
}