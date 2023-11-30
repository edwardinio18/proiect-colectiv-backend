namespace MioriticMindsAPI.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int UserId { get; set; }
    }
}