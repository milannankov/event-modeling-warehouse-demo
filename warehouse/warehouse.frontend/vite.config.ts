import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Aspire injects the backend URL as services__warehouse__http__0
const backendUrl = process.env.services__warehouse__http__0 ?? 'http://localhost:5001'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: backendUrl,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
