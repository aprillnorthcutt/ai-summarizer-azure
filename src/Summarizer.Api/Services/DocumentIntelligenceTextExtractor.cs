using Azure;
using Azure.AI.DocumentIntelligence;

namespace Summarizer.Api.Services;

public class DocumentIntelligenceTextExtractor : ITextExtractor
{
    private readonly DocumentIntelligenceClient _client;
    public DocumentIntelligenceTextExtractor(DocumentIntelligenceClient client) => _client = client;

    public async Task<string> ExtractTextAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        // Read the upload stream into BinaryData safely (no ReadTimeout access)
        BinaryData binaryContent = await BinaryData.FromStreamAsync(content, ct);

        // Try fast read model first; fall back to layout for tricky docs
        string text = await AnalyzeToTextAsync(binaryContent, "prebuilt-read", ct);
        if (string.IsNullOrWhiteSpace(text) || text.Length < 50)
        {
            text = await AnalyzeToTextAsync(binaryContent, "prebuilt-layout", ct);
        }

        return text ?? string.Empty;
    }

    private async Task<string> AnalyzeToTextAsync(BinaryData binary, string modelId, CancellationToken ct)
    {
        var poller = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, binary, ct);
        var result = poller.Value;

        var sb = new System.Text.StringBuilder();

        // Prefer lines
        if (result.Pages is not null)
        {
            foreach (var page in result.Pages)
            foreach (var line in page.Lines)
                sb.AppendLine(line.Content);
        }

        var text = sb.ToString();
        if (!string.IsNullOrWhiteSpace(text))
            return text;

        // Fallback to paragraphs if lines were empty
        sb.Clear();
        if (result.Paragraphs is not null)
        {
            foreach (var paragraph in result.Paragraphs)
                sb.AppendLine(paragraph.Content);
        }

        return sb.ToString();
    }
}