import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const isAzure = process.env.USE_AZURE === 'true';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/summarize': {
        target: isAzure
          ? 'https://keywordvista.azurewebsites.net'
          : 'https://localhost:7001',
        changeOrigin: true,
        secure: false, // Allow self-signed localhost SSL
      },
      '/antiforgery': {
        target: isAzure
          ? 'https://keywordvista.azurewebsites.net'
          : 'https://localhost:7001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    outDir: '../wwwroot',     // Where ASP.NET expects the build output
    emptyOutDir: true,        // Clean wwwroot on each build
  },
})
