export const buildPath = (id: string) => `/b/${encodeURIComponent(id)}`

export const newBuildPath = (championId: string) => `/nowy/${encodeURIComponent(championId)}`

export const shareUrl = (id: string) => `${window.location.origin}${buildPath(id)}`
