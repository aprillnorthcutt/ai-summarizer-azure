import React, { useState } from "react";
import { postSummarizeText } from "../api";

export default function AbstractiveSummarizer() {
  const [input, setInput] = useState("");
  const [output, setOutput] = useState<string>("");

  async function handleSummarize() {
    setOutput("");
    const res = await postSummarizeText(input);
    const data = await res.json().catch(() => ({}));
    if (!res.ok) {
      setOutput(`Error ${res.status}: ${data?.message ?? "Request failed"}`);
      return;
    }
    setOutput(data.summary ?? JSON.stringify(data, null, 2));
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
      {output && (
        <pre className="mt-4 p-3 border rounded bg-gray-100 text-black whitespace-pre-wrap">
          {output}
        </pre>
      )}
    </div>
  );
}
