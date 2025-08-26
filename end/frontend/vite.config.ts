import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '#shared': path.resolve(__dirname, '../shared/src'),
      '#components': path.resolve(__dirname, 'src/components'),
      '#utils': path.resolve(__dirname, 'src/utils'),
    },
  },
});