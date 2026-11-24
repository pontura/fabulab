import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  // Relative paths: the built site (dist/) works from any folder of the server, not only from its root
  base: './',
})
