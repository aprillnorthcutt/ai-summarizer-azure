import React from "react";

export default function Hero() {
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
