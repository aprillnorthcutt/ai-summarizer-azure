using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Summarizer.Api.Models
{
    public sealed class SummarizeTextForm
    {
        [FromForm(Name = "text")]
        [SwaggerSchema(
            Description = "Paste a paragraph or longer text to summarize.",
            Format = "textarea"  // Makes Swagger show a large multi-line box
        )]
        public string Text { get; set; } = string.Empty;

    }
}