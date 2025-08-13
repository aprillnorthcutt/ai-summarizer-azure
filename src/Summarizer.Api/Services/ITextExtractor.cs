namespace Summarizer.Api.Services;

public interface ITextExtractor
{
    Task<string> ExtractTextAsync(Stream content, string fileName, CancellationToken ct = default);
}