# AI Summarizer API – Developer Guide

An ASP.NET Core minimal API that integrates with Azure AI services to summarize text and documents. Includes Swagger UI for interactive testing, with full support for file uploads.

---

## ✨ Overview

**Tech Stack:** .NET 8 • ASP.NET Core Minimal APIs • Swashbuckle (Swagger) • Azure AI Language • Azure Document Intelligence

**Endpoints:**

* **`POST /summarize/text`** → Summarizes raw text.
* **`POST /summarize/document`** → Summarizes uploaded documents (`multipart/form-data`).

**Configuration:** Uses environment variables for Azure endpoints and keys — no secrets files needed.

---

## ⚙️ Setup

### Required Environment Variables

| Variable               | Description                               |
| ---------------------- | ----------------------------------------- |
| `AI_LANGUAGE_ENDPOINT` | Azure AI Language endpoint URL.           |
| `AI_LANGUAGE_KEY`      | Azure AI Language API key.                |
| `AI_DOCUMENT_ENDPOINT` | Azure Document Intelligence endpoint URL. |
| `AI_DOCUMENT_KEY`      | Azure Document Intelligence API key.      |

> If any variable is missing, the API will fail at startup with a clear error message.

---

## 🚀 Running Locally

1. **Set environment variables** for your Azure services.
2. **Run the API:**

   ```bash
   dotnet run
   ```
3. **Open Swagger UI:**
   Navigate to `https://localhost:<port>/swagger` in your browser to test endpoints.

---

## 📌 Endpoints & Examples

### **`POST /summarize/text`**

**Request Body (JSON):**

```json
{
  "text": "Your text here",
  "maxSentences": 3
}
```

**Sample Response:**

```json
{
  "summary": "string",
  "sentences": ["string"],
  "model": "azure-ai-language",
  "durationMs": 123
}
```

---

### **`POST /summarize/document`**

**Form-data Fields:**

* **`file`** *(required)* → Document to summarize.
* **`maxSentences`** *(optional)* → Maximum number of sentences in the summary.

**Sample Response:**

```json
{
  "summary": "string",
  "pages": 10,
  "model": "azure-document-intelligence",
  "durationMs": 456
}
```

---

## 🖥️ Swagger File Upload Support

Use this **operation filter** to enable file uploads in Swagger for Minimal APIs:

```csharp
public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFileParam = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile));
        if (!hasFileParam) return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                        {
                            ["file"] = new OpenApiSchema { Type = "string", Format = "binary" },
                            ["maxSentences"] = new OpenApiSchema { Type = "integer", Nullable = true }
                        },
                        Required = new HashSet<string> { "file" }
                    }
                }
            }
        };
    }
}
```

**Register the filter:**

```csharp
builder.Services.AddSwaggerGen(c => c.OperationFilter<SwaggerFileOperationFilter>());
```

---

## 🛠️ Service Example

```csharp
public class TextSummarizationService
{
    private readonly TextAnalyticsClient _client;
    public TextSummarizationService(TextAnalyticsClient client) => _client = client;

    public async Task<SummaryResponse> SummarizeAsync(string text, int? maxSentences)
    {
        // Call Azure AI summarization here
        return new SummaryResponse { Summary = "..." };
    }
}
```

---

## ❗ Troubleshooting

* **Missing env vars:** Check all four environment variables are set.
* **Swagger file picker missing:** Ensure `SwaggerFileOperationFilter` is registered.
* **415 Unsupported Media Type:** Use `multipart/form-data` for file uploads.

---

[⬅ Back to Main Project README](../../README.md)
