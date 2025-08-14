using Summarizer.Api.Models;

namespace Summarizer.Api.Services;

public interface ISummarizer
{
    Task<SummarizeResponse> SummarizeAsync(string text, CancellationToken ct = default);
}