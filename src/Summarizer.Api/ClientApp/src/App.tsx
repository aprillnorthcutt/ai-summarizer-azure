import React, { useEffect, useState } from "react";
import {
  initAntiforgery,
  postSummarizeText,
  postSummarizeDocument,
  postSummarizeAbstractive,
} from "./api";
import { Tabs, Tab } from "./components/Tabs";


function Header() {
  return (
    <header className="sticky top-0 z-20 w-full border-b bg-white/70 backdrop-blur dark:bg-gray-950/70">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-2xl bg-gradient-to-tr from-indigo-500 to-sky-500" />
          <span className="text-lg font-semibold tracking-tight">AI Summarizer</span>
        </div>
         </div>
    </header>
  );
}

function Hero() {
  return (
    <section className="text-center py-10 md:py-14">
      <h1 className="text-3xl md:text-4xl font-bold tracking-tight">Summarize text and documents fast</h1>
      <p className="mt-3 md:mt-4 text-base md:text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
        Paste text or upload a document. Get a clean, readable summary in seconds.
      </p>
      <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">Backed by the same API used in Swagger.</p>
    </section>
  );
}

export default function App() {
  const [activeTab, setActiveTab] = useState(0);
  const [textInput, setTextInput] = useState("");
  const [fileInput, setFileInput] = useState<File | null>(null);
  const [output, setOutput] = useState("");
  const [keywords, setKeywords] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const exampleText = `Extractive summarization identifies and selects key sentences or phrases directly from the original text, assembling them into a condensed version without altering the wording. It functions like a highlighter, pinpointing segments that carry core meaning and presenting them verbatim. This method often relies on statistical cues such as term frequency or sentence position to determine relevance. Because it preserves the original language, extractive summaries are typically factually consistent and grammatically intact, though they may lack narrative cohesion. It’s especially useful in domains like legal, medical, or technical documentation where fidelity to source language is essential.

Abstractive summarization, by contrast, rephrases and synthesizes ideas using new language, aiming to capture the essence of the content rather than replicate it. This approach mimics human summarization—understanding meaning and retelling it in original words. Powered by deep learning models like transformers, abstractive methods offer more fluent and readable outputs but carry a higher risk of factual distortion. They’re ideal for applications like news aggregation or executive briefings, where clarity and narrative flow are prioritized over strict textual fidelity.`;


  useEffect(() => {
    initAntiforgery().catch(() => {});
  }, []);


  function formatOutputFromData(data: any) {
    const root = data?.analysis ?? data ?? {};
    const summary: string = root.summary ?? "";
    const keywords: string[] = root.keywords ?? [];
    const parts: string[] = [];
    if (summary) parts.push(summary);

	if (keywords.length) {
  setKeywords(keywords); // this updates your bubble state
}

    const timings = data?.timings ?? root?.timings;
    if (timings && typeof timings === "object") {
      const ms = Object.entries(timings)
        .map(([k, v]) => `${k}=${v}ms`)
        .join(", ");
      if (ms) parts.push(`\n\nTimings: ${ms}`);
    }

    const file = data?.file ?? root?.file;
    if (file?.name) parts.push(`\nFile: ${file.name}${file.length ? ` (${file.length} bytes)` : ""}`);

    return parts.length ? parts.join("") : JSON.stringify(data, null, 2);
  }

  async function handleTextSubmit() {
    try {
      setLoading(true);
      setOutput("");
      const res = await postSummarizeText(textInput);
      if (!res.ok) {
        setOutput(await res.text());
        return;
      }
      const data = await res.json().catch(() => ({}));
      setOutput(formatOutputFromData(data));
    } finally {
      setLoading(false);
    }
  }

  async function handleFileSubmit() {
    if (!fileInput) return;
    try {
      setLoading(true);
      setOutput("");
      const res = await postSummarizeDocument(fileInput);
      if (!res.ok) {
        setOutput(await res.text());
        return;
      }
      const data = await res.json().catch(() => ({}));
      setOutput(formatOutputFromData(data));
    } finally {
      setLoading(false);
    }
  }

async function handleAbstractiveSubmit() {
  try {
    setLoading(true);
    setOutput("");
    const res = await postSummarizeAbstractive(textInput, 6); // tweak 6 as you like
    if (!res.ok) { setOutput(await res.text()); return; }
    const data = await res.json().catch(() => ({}));
	setOutput(formatOutputFromData(data));

  } finally { setLoading(false); }
}


  return (
    <div className="min-h-screen bg-gray-50 text-gray-900 dark:bg-gray-950 dark:text-gray-100">
      <Header />
      <main className="mx-auto max-w-5xl px-4 pb-24">
        <Hero />

        <section className="mt-6 rounded-2xl border bg-white p-4 shadow-sm dark:border-gray-800 dark:bg-gray-900">

<Tabs setOutput={setOutput}>
  <Tab label="Extractive Summarize Text">
    <label className="mb-2 block text-sm text-gray-600 dark:text-gray-300">
      Text to summarize
    </label>
    <textarea
      className="w-full min-h-[180px] resize-vertical rounded-lg border border-gray-300 bg-white p-3 pt-4 text-gray-900 placeholder-gray-400 focus:border-indigo-500 focus:outline-none focus:ring focus:ring-indigo-500/30 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100 dark:placeholder-gray-500"
      placeholder="Paste text here…"
      value={textInput}
      onChange={(e) => setTextInput(e.target.value)}
    />
    <button
      onClick={() => {
        setTextInput(exampleText);
        setOutput("");
        setKeywords([]);
      }}
      className="mt-2 text-sm text-blue-600 hover:underline"
    >
      Use Example Text
    </button>
    <button
      onClick={handleTextSubmit}
      className="mt-3 ml-3 rounded-lg bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700 disabled:opacity-50"
      disabled={loading || !textInput.trim()}
    >
      {loading ? "Summarizing..." : "Summarize Text"}
    </button>
  </Tab>

  <Tab label="Extractive Summarize Document (.pdf, .png, .docx)">
    <label className="mb-2 block text-sm text-gray-600 dark:text-gray-300">
      Upload a document
    </label>
    <input
      type="file"
      accept=".pdf,.png,.jpg,.jpeg,.docx"
      onChange={(e) => setFileInput(e.target.files?.[0] ?? null)}
      className="block w-full rounded-lg border border-gray-300 bg-white p-2 text-gray-900 file:mr-3 file:rounded-md file:border-0 file:bg-gray-200 file:px-3 file:py-2 file:text-gray-900 hover:file:bg-gray-300 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100 dark:file:bg-gray-800 dark:hover:file:bg-gray-700"
    />
    <button
      onClick={handleFileSubmit}
      className="mt-3 rounded-lg bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700 disabled:opacity-50"
      disabled={loading || !fileInput}
    >
      {loading ? "Summarizing..." : "Summarize Document"}
    </button>
  </Tab>

  <Tab label="Abstractive Summarize Text">
    <label className="mb-2 block text-sm text-gray-600 dark:text-gray-300">
      Text to summarize
    </label>
    <textarea
      className="w-full min-h-[180px] resize-vertical rounded-lg border border-gray-300 bg-white p-3 pt-4 text-gray-900 placeholder-gray-400 focus:border-indigo-500 focus:outline-none focus:ring focus:ring-indigo-500/30 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100 dark:placeholder-gray-500"
      placeholder="Paste text here…"
      value={textInput}
      onChange={(e) => setTextInput(e.target.value)}
    />
    <button
      onClick={() => {
        setTextInput(exampleText);
        setOutput("");
        setKeywords([]);
      }}
      className="mt-2 text-sm text-blue-600 hover:underline"
    >
      Use Example Text
    </button>
    <button
      onClick={handleAbstractiveSubmit}
      className="mt-3 ml-3 rounded-lg bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700 disabled:opacity-50"
      disabled={loading || !textInput.trim()}
    >
      {loading ? "Summarizing..." : "Summarize (Abstractive)"}
    </button>
  </Tab>
</Tabs>

			{output && (
  <div className="mt-6 space-y-4">
    <div className="whitespace-pre-wrap rounded-lg border border-gray-200 bg-gray-50 p-3 text-sm text-gray-900 dark:border-gray-700 dark:bg-gray-950 dark:text-gray-100">
      {output}
    </div>

    {keywords.length > 0 && (
  <div className="mt-4">
    <div className="mb-2 text-sm font-semibold text-gray-700 dark:text-gray-300">Keywords:</div>
    <div className="flex flex-wrap gap-2">
      {keywords.map((word, idx) => (
        <span
          key={idx}
          className="rounded-full bg-indigo-100 px-3 py-1 text-sm font-medium text-indigo-800 dark:bg-indigo-900 dark:text-indigo-100"
        >
          {word}
        </span>
      ))}
    </div>
  </div>
)}

  </div>
)}
        </section>
      </main>
    </div>
  );
}
