// Riot IDs are "Name#TAG"; the tag never contains '#', so the last one splits them
export function splitRiotId(riotId: string): { name: string; tag: string } | null {
  const at = riotId.lastIndexOf('#')
  if (at <= 0 || at === riotId.length - 1) return null
  return { name: riotId.slice(0, at), tag: riotId.slice(at + 1) }
}

export function playerPath(riotId: string): string | null {
  const id = splitRiotId(riotId)
  return id && `/gracz/${encodeURIComponent(id.name)}/${encodeURIComponent(id.tag)}`
}
