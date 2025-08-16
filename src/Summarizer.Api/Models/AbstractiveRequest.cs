namespace Summarizer.Api.Models
{
    public class AbstractiveRequest
    {
        public string Text { get; set; } = string.Empty;
        public int SentenceCount { get; set; } = 3; // Optional, just for prompt context
    }
}
