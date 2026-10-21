import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // `func start` in src/Web.Api listens on 7071; in Azure the API URL comes from VITE_API_URL
    proxy: { '/api': 'http://localhost:7071' },
  },
})
