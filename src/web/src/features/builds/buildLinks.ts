import { apiUrl } from '../../api/apiUrl'

export const buildPath = (id: string) => `/b/${encodeURIComponent(id)}`

export const newBuildPath = (championId: string) => `/nowy/${encodeURIComponent(championId)}`

// The API's preview page: Discord reads the build's name, runes and splash art from it, people land on buildPath(id).
// Locally the API is behind Vite's /api proxy, so the site's own origin works too
export const shareUrl = (id: string) => `${apiUrl || window.location.origin}/api/share/${encodeURIComponent(id)}`

export const buildsPath = '/buildy'

// The team's builds of one champion, with the button to make a new one
export const championBuildsPath = (championId: string) => `${buildsPath}/${encodeURIComponent(championId)}`
