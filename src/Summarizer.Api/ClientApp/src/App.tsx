import React, { useEffect, useState } from "react";
import {
  initAntiforgery,
  postSummarizeText,
  postSummarizeDocument,
  postSummarizeAbstractive,
} from "./api";

function Header() {
  return (
    <header className="sticky top-0 z-20 w-full border-b bg-white/70 backdrop-blur dark:bg-gray-950/70">
      <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-2xl bg-gradient-to-tr from-indigo-500 to-sky-500" />
          <span className="text-lg font-semibold tracking-tight">AI Summarizer</span>
        </div>
        <a
          className="rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-100 dark:border-gray-700 dark:hover:bg-gray-800"
          href="/swagger/index.html"
          target="_blank"
          rel="noreferrer"
        >
          Open Swagger
        </a>
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

function Tabs({ active, setActive }: { active: number; setActive: (i: number) => void }) {
  const labels = ["Summarize Text", "Summarize Document (.pdf, .png, .docx)", "Abstractive"];
  return (
    <div className="flex gap-2 mb-4">
      {labels.map((label, i) => (
        <button
          key={i}
          onClick={() => setActive(i)}
          className={`rounded-full px-4 py-2 text-sm transition ${
            i === active
              ? "bg-indigo-600 text-white font-semibold"
              : "text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800/70"
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}

export default function App() {
  const [activeTab, setActiveTab] = useState(0);
  const [textInput, setTextInput] = useState("");
  const [fileInput, setFileInput] = useState<File | null>(null);
  const [output, setOutput] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    initAntiforgery().catch(() => {});
  }, []);

  // Normalize API responses (flat or { analysis: {...} })
  function formatOutputFromData(data: any) {
    const root = data?.analysis ?? data ?? {};
    const summary: string = root.summary ?? "";
    const keywords: string[] = root.keywords ?? [];
    const parts: string[] = [];
    if (summary) parts.push(summary);
    if (keywords.length) parts.push(`\n\nKeywords: ${keywords.join(", ")}`);

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
    //setOutput(data.summary ?? JSON.stringify(data, null, 2));
setOutput(formatOutputFromData(data));

  } finally { setLoading(false); }
}


  return (
    <div className="min-h-screen bg-gray-50 text-gray-900 dark:bg-gray-950 dark:text-gray-100">
      <Header />
      <main className="mx-auto max-w-5xl px-4 pb-24">
        <Hero />

        <section className="mt-6 rounded-2xl border bg-white p-4 shadow-sm dark:border-gray-800 dark:bg-gray-900">
          <Tabs active={activeTab} setActive={setActiveTab} />

          {activeTab === 0 && (
            <div>
              <label className="mb-2 block text-sm text-gray-600 dark:text-gray-300">Text to summarize</label>
              <textarea
                className="w-full min-h-[180px] resize-vertical rounded-lg border border-gray-300 bg-white p-3 pt-4 text-gray-900 placeholder-gray-400 focus:border-indigo-500 focus:outline-none focus:ring focus:ring-indigo-500/30 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100 dark:placeholder-gray-500"
                placeholder="Paste text here…"
                value={textInput}
                onChange={(e) => setTextInput(e.target.value)}
              />
              <button
                onClick={handleTextSubmit}
                className="mt-3 rounded-lg bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700 disabled:opacity-50"
                disabled={loading || !textInput.trim()}
              >
                {loading ? "Summarizing..." : "Summarize Text"}
              </button>
            </div>
          )}

          {activeTab === 1 && (
            <div>
              <label className="mb-2 block text-sm text-gray-600 dark:text-gray-300">Upload a document</label>
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
            </div>
          )}

          {activeTab === 2 && (
            <div>
              <label className="mb-2 block text-sm text-gray-600 dark:text-gray-300">Abstractive input</label>
              <textarea
                className="w-full min-h-[180px] resize-vertical rounded-lg border border-gray-300 bg-white p-3 pt-4 text-gray-900 placeholder-gray-400 focus:border-indigo-500 focus:outline-none focus:ring focus:ring-indigo-500/30 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-100 dark:placeholder-gray-500"
                placeholder="Paste text here…"
                value={textInput}
                onChange={(e) => setTextInput(e.target.value)}
              />
              <button
                onClick={handleAbstractiveSubmit}
                className="mt-3 rounded-lg bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700 disabled:opacity-50"
                disabled={loading || !textInput.trim()}
              >
                {loading ? "Summarizing..." : "Summarize (Abstractive)"}
              </button>
            </div>
          )}

          {output && (
            <pre className="mt-6 whitespace-pre-wrap rounded-lg border border-gray-200 bg-gray-50 p-3 text-sm text-gray-900 dark:border-gray-700 dark:bg-gray-950 dark:text-gray-100">
              {output}
            </pre>
          )}
        </section>
      </main>
    </div>
  );
}
