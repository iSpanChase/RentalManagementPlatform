import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    allowedHosts: [
      'localhost',
      '.ngrok.app',
      '.ngrok.io',
      'my-project-frontend.ngrok.app'
    ],
    proxy: {
      '/api': {
        target: 'https://localhost:7230',
        changeOrigin: true,
        secure: false,
      },
      '/notificationHub': {
        target: 'https://localhost:7230',
        changeOrigin: true,
        secure: false,
        ws: true
      }
    }
  }
})
