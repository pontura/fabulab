import { createReadStream, existsSync, statSync } from 'node:fs'
import { resolve, sep } from 'node:path'
import type { Connect, Plugin } from 'vite'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The Unity WebGL builds (public/player, editor, minigames) are compressed (Brotli: *.wasm.br, *.data.br...). The
// browser only decompresses them if they come with Content-Encoding, as public/.htaccess does on the server: this
// does the same in the dev server and in vite preview
const contentTypes: Record<string, string> = {
  wasm: 'application/wasm',
  js: 'application/javascript',
}

function compressedFiles(root: string): Connect.NextHandleFunction {
  return (req, res, next) => {
    const path = decodeURIComponent((req.url ?? '').split('?')[0])
    const match = path.match(/\.(\w+)\.(br|gz)$/)
    if (!match) return next()
    const file = resolve(root, '.' + path)
    if (!file.startsWith(root + sep) || !existsSync(file)) return next()
    res.setHeader('Content-Encoding', match[2] === 'br' ? 'br' : 'gzip')
    res.setHeader('Content-Type', contentTypes[match[1]] ?? 'application/octet-stream')
    res.setHeader('Content-Length', statSync(file).size)
    res.setHeader('Cache-Control', 'no-cache')
    if (req.method === 'HEAD') return res.end()
    createReadStream(file).pipe(res)
  }
}

function unityBuilds(): Plugin {
  return {
    name: 'unity-compressed-builds',
    configureServer(server) {
      server.middlewares.use(compressedFiles(resolve(server.config.publicDir)))
    },
    configurePreviewServer(server) {
      server.middlewares.use(compressedFiles(resolve(server.config.build.outDir)))
    },
  }
}

export default defineConfig({
  plugins: [react(), unityBuilds()],
  // Relative paths: the built site (dist/) works from any folder of the server, not only from its root
  base: './',
})
