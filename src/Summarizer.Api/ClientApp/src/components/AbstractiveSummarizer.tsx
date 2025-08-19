import React, { useState } from "react";
import { postSummarizeText } from "../api";

export default function AbstractiveSummarizer() {
  const [input, setInput] = useState("");
  const [summary, setSummary] = useState<string | null>(null);
  const [keywords, setKeywords] = useState<string[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function handleSummarize() {
    setSummary(null);
    setKeywords(null);
    setError(null);

    try {
      const res = await postSummarizeText(input);
      const data = await res.json();

      if (!res.ok) {
        setError(`Error ${res.status}: ${data?.message ?? "Request failed"}`);
        return;
      }

      setSummary(data.summary ?? "No summary returned.");
      setKeywords(Array.isArray(data.keywords) ? data.keywords : []);
    } catch (err) {
      setError("An unexpected error occurred.");
    }
  }

  return (
    <div>
      <textarea
        className="w-full p-2 border rounded mb-2 text-black"
        rows={8}
        value={input}
        onChange={(e) => setInput(e.target.value)}
        placeholder="Abstractive mode: paste text…"
      />
      <button
        onClick={handleSummarize}
        className="px-4 py-2 bg-blue-600 text-white rounded"
      >
        Summarize (Abstractive)
      </button>

      {error && (
        <div className="mt-4 p-3 border rounded bg-red-100 text-red-800">
          {error}
        </div>
      )}

      {summary && (
        <div className="mt-4 p-3 border rounded bg-gray-100 text-black whitespace-pre-wrap">
          <strong>Summary:</strong>
          <p>{summary}</p>
        </div>
      )}

      {keywords && keywords.length > 0 && (
        <div className="mt-4 p-3 border rounded bg-gray-100 text-black">
          <strong>Keywords:</strong>
          <ul className="list-disc list-inside">
            {keywords.map((kw, idx) => (
              <li key={idx}>{kw.trim()}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
