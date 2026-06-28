import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    // @vitejs/plugin-react v6 transforms with oxc and dropped its `babel` option,
    // so Relay's graphql`` tags are compiled in with a dedicated Babel pass.
    // preset-typescript lets Babel parse .ts/.tsx (it leaves JSX for the React/oxc pass).
    babel({
      presets: ['@babel/preset-typescript'],
      plugins: ['relay'],
    }),
    react(),
  ],
  server: {
    // Reachable through the Aspire/YARP gateway, which forwards the internal host header.
    allowedHosts: ['localhost', '.aspire.dev.internal'],
  },
})
