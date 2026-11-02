/// <reference types="vitest/config" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: { '/todos': 'http://localhost:5000' },
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
  },
});