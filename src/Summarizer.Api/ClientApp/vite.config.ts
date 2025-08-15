import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // forward all API calls to your Azure Web App during dev
      '/summarize': {
        target: 'https://keywordvista.azurewebsites.net',
        changeOrigin: true,
        secure: true,
      },
      '/swagger': {
        target: 'https://keywordvista.azurewebsites.net',
        changeOrigin: true,
        secure: true,
      },
    },
  },
})
