const apiUrl = import.meta.env.VITE_API_URL ?? ''

export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${apiUrl}/api/${path}`, { signal })
  if (!response.ok) throw new Error(`GET /api/${path} failed: ${response.status}`)
  return (await response.json()) as T
}
