using System.Linq;
using Azure.AI.TextAnalytics;
using Summarizer.Api.Models;

namespace Summarizer.Api.Services;

public class LanguageSummarizer : ISummarizer
{
    private readonly TextAnalyticsClient _client;
    public LanguageSummarizer(TextAnalyticsClient client) => _client = client;

    public async Task<SummarizeResponse> SummarizeAsync(string text, CancellationToken ct = default)
    {
        var originalLen = text?.Length ?? 0;
        if (string.IsNullOrWhiteSpace(text))
            return new SummarizeResponse("", Array.Empty<string>(), 0, 0);

        // chunk long inputs to respect service limits (~5k chars/doc is safe)
        var docs = Chunk(text, 4500).ToList();
        var batch = docs.Select(d => new TextDocumentInput(Guid.NewGuid().ToString(), d)).ToList();

        var actions = new TextAnalyticsActions
        {
            // IMPORTANT in 5.3.x GA: assign a new List<>
            ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>
            {
                new ExtractiveSummarizeAction()
            }
        };

        var op = await _client.StartAnalyzeActionsAsync(batch, actions, cancellationToken: ct);
        await op.WaitForCompletionAsync(ct);

        var summaryPieces = new List<string>();
        var keyPhrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await foreach (var page in op.Value)
        {
            foreach (var actionResult in page.ExtractiveSummarizeResults)
            {
                if (actionResult.HasError) continue;

                foreach (var docResult in actionResult.DocumentsResults)
                {
                    if (docResult.HasError) continue;

                    // Take top-ranked sentences (tune as desired)
                    var ordered = docResult.Sentences.OrderBy(s => s.RankScore);
                    summaryPieces.Add(string.Join(" ", ordered.Take(10).Select(s => s.Text)));
                }
            }
        }

        // Key phrases
        var kpResponse = await _client.ExtractKeyPhrasesBatchAsync(batch.Select(b => b.Text), cancellationToken: ct);
        foreach (var doc in kpResponse.Value)
        {
            if (doc.HasError) continue;
            foreach (var p in doc.KeyPhrases) keyPhrases.Add(p);
        }

        var summary = string.Join("\n\n", summaryPieces.Where(s => !string.IsNullOrWhiteSpace(s)));
        return new SummarizeResponse(
            Summary: summary,
            KeyPhrases: keyPhrases.ToList(),
            OriginalCharLength: originalLen,
            SummaryCharLength: summary.Length
        );
    }

    private static IEnumerable<string> Chunk(string str, int max)
    {
        for (int i = 0; i < str.Length; i += max)
            yield return str.Substring(i, Math.Min(max, str.Length - i));
    }
}
