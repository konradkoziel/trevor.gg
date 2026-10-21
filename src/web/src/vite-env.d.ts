/// <reference types="vite/client" />

interface ImportMetaEnv {
  // Base URL of the Function App with the API, set by the deploy workflow; empty locally (Vite proxies /api)
  readonly VITE_API_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
