import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { fileURLToPath, URL } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  base: './',
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 81, // Change this to your desired port
  },
  define: {
    // Inject build timestamp at build time
    __BUILD_TIME__: JSON.stringify(new Date().toISOString()),
  },
})
