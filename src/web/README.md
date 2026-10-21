# Trevor.gg website (React + Vite + TypeScript)

```bash
npm ci
npm run dev     # http://localhost:5173, /api is proxied to the Web.Api Function App on :7071
npm test        # vitest
npm run lint    # oxlint
npm run build   # type check + production build into dist/
```

`VITE_API_URL` (set by `.github/workflows/web.yml`) is the API's base URL in Azure. See the root `readme.md` for the architecture.
