import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forward API calls to your Azure Web App
      "/summarize": {
        target: "https://keywordvista.azurewebsites.net",
        changeOrigin: true,
        secure: true,
        cookieDomainRewrite: "localhost", // rewrite cookie domain for local dev
        cookiePathRewrite: "/",           // normalize path
      },
      "/antiforgery": {
        target: "https://keywordvista.azurewebsites.net",
        changeOrigin: true,
        secure: true,
        cookieDomainRewrite: "localhost",
        cookiePathRewrite: "/",
      },
      "/swagger": {
        target: "https://keywordvista.azurewebsites.net",
        changeOrigin: true,
        secure: true,
      },
    },
  },
  resolve: {
    alias: {
      "@": "/src", // optional: allows you to import like "@/components/..."
    },
  },
});
