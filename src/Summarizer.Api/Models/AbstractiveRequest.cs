namespace Summarizer.Api.Models
{
    public class AbstractiveRequest
    {
        public string? Text { get; set; }
        public int? SentenceCount { get; set; }
    }

    //[FromForm(Name = "text")]
    //[SwaggerSchema(
    //    Description = "Paste a paragraph or longer text to summarize.",
    //    Format = "textarea"  // Makes Swagger show a large multi-line box
    //)]
    //public string Text { get; set; } = string.Empty;
}
