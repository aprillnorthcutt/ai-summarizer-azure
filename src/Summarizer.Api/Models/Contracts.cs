namespace Summarizer.Api.Models;

public record SummarizeRequest(string Text);

public record SummarizeResponse(
    string Summary,
    IReadOnlyList<string> KeyPhrases,
    int OriginalCharLength,
    int SummaryCharLength
);