import React, { useState } from "react";
import { postSummarizeDocument } from "../api";

export default function DocumentSummarizer() {
  const [file, setFile] = useState<File | null>(null);
  const [output, setOutput] = useState<string>("");

  async function handleSummarize() {
    if (!file) return;
    setOutput("");
    const res = await postSummarizeDocument(file);
    const data = await res.json().catch(() => ({}));
    if (!res.ok) {
      setOutput(`Error ${res.status}: ${data?.message ?? "Request failed"}`);
      return;
    }
    setOutput(data.summary ?? JSON.stringify(data, null, 2));
  }

  return (
    <div>
      <input
        type="file"
        accept=".pdf,.png,.jpg,.jpeg,.docx"
        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        className="mb-2"
      />
      <div>
        <button
          disabled={!file}
          onClick={handleSummarize}
          className="px-4 py-2 bg-blue-600 text-white rounded disabled:opacity-50"
        >
          Summarize Document
        </button>
      </div>
      {output && (
        <pre className="mt-4 p-3 border rounded bg-gray-100 text-black whitespace-pre-wrap">
          {output}
        </pre>
      )}
    </div>
  );
}
