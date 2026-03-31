import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

export default defineConfig({
  plugins: [svelte()],
  build: {
    target: 'es2020',
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          if (id.includes('chart.js')) return 'chartjs'
        }
      }
    }
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5297',
        changeOrigin: true,
      }
    }
  }
})
